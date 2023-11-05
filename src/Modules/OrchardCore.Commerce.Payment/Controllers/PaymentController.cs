using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.Constants;
using OrchardCore.Commerce.Exceptions;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.MoneyDataType.Extensions;
using OrchardCore.Commerce.Payment;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.ViewModels;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Mvc.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Controllers;

public class PaymentController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<PaymentController> _logger;
    private readonly IContentManager _contentManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IStringLocalizer T;
    private readonly IHtmlLocalizer<PaymentController> H;
    private readonly INotifier _notifier;
    private readonly IMoneyService _moneyService;
    private readonly IEnumerable<IPaymentProvider> _paymentProviders;
    private readonly IPaymentService _paymentService;

    public PaymentController(
        IOrchardServices<PaymentController> services,
        IUpdateModelAccessor updateModelAccessor,
        INotifier notifier,
        IMoneyService moneyService,
        IEnumerable<IPaymentProvider> paymentProviders,
        IPaymentService paymentService)
    {
        _authorizationService = services.AuthorizationService.Value;
        _logger = services.Logger.Value;
        _contentManager = services.ContentManager.Value;
        _updateModelAccessor = updateModelAccessor;
        T = services.StringLocalizer.Value;
        H = services.HtmlLocalizer.Value;
        _notifier = notifier;
        _moneyService = moneyService;
        _paymentProviders = paymentProviders;
        _paymentService = paymentService;
    }

    [Route("checkout")]
    public async Task<IActionResult> Index(string? shoppingCartId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.Checkout))
        {
            return User.Identity?.IsAuthenticated == true ? Forbid() : LocalRedirect("~/Login?ReturnUrl=~/checkout");
        }

        if (await _paymentService.CreateCheckoutViewModelAsync(shoppingCartId) is not { } checkoutViewModel)
        {
            return Redirect("~/cart/empty");
        }

        foreach (dynamic shape in checkoutViewModel.CheckoutShapes) shape.ViewModel = checkoutViewModel;

        checkoutViewModel.Provinces.AddRange(Regions.Provinces);

        return View(checkoutViewModel);
    }

    [Route("checkout/price")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Price(string? shoppingCartId) =>
        await this.SafeJsonAsync(async () =>
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.Checkout))
            {
                throw new InvalidOperationException("Unauthorized.");
            }

            var updater = _updateModelAccessor.ModelUpdater;
            var shippingViewModel = new AddressFieldViewModel();
            var billingViewModel = new AddressFieldViewModel();
            if (!await updater.TryUpdateModelAsync(shippingViewModel, $"{nameof(OrderPart)}.{nameof(OrderPart.ShippingAddress)}") ||
                !await updater.TryUpdateModelAsync(billingViewModel, $"{nameof(OrderPart)}.{nameof(OrderPart.BillingAddress)}"))
            {
                throw new InvalidOperationException(
                    _updateModelAccessor.ModelUpdater.GetModelErrorMessages().JoinNotNullOrEmpty());
            }

            var checkoutViewModel = await _paymentService.CreateCheckoutViewModelAsync(
                shoppingCartId,
                part =>
                {
                    part.ShippingAddress.Address = shippingViewModel.Address ?? part.ShippingAddress.Address;
                    part.BillingAddress.Address = billingViewModel.Address ?? part.BillingAddress.Address;
                });

            var total = checkoutViewModel?.SingleCurrencyTotal ?? new Amount(0, _moneyService.DefaultCurrency);

            return new
            {
                total.Value,
                Currency = total.Currency.CurrencyIsoCode,
                Text = total.ToString(),
            };
        });

    [Route("checkout/validate/{providerName}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Validate(string providerName)
    {
        if (string.IsNullOrEmpty(providerName)) return NotFound();

        try
        {
            await _paymentProviders
                .WhereName(providerName)
                .AwaitEachAsync(provider => provider.ValidateAsync(_updateModelAccessor));

            var errors = _updateModelAccessor.ModelUpdater.GetModelErrorMessages().ToList();
            return Json(new { Errors = errors });
        }
        catch (FrontendException exception)
        {
            return Json(new { Errors = new[] { exception.HtmlMessage.Html() } });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An exception has occurred during checkout form validation.");

            var errorMessage = HttpContext.IsDevelopmentAndLocalhost()
                ? exception.ToString()
                : T["An exception has occurred during checkout form validation."].Value;

            return Json(new { Errors = new[] { errorMessage } });
        }
    }

    [Route("checkout/paymentrequest/{orderId}")]
    public async Task<IActionResult> PaymentRequest(string orderId)
    {
        if (await _contentManager.GetAsync(orderId) is not { } order ||
            order.As<OrderPart>() is not { } orderPart)
        {
            return NotFound();
        }

        if (string.IsNullOrEmpty(User.Identity?.Name))
        {
            return LocalRedirect($"~/Login?ReturnUrl=~/Contents/ContentItems/{orderId}");
        }

        // If there are no line items, there is nothing to be done.
        if (!orderPart.LineItems.Any())
        {
            await _notifier.InformationAsync(H["This Order contains no line items, so there is nothing to be paid."]);
            return this.RedirectToContentDisplay(orderId);
        }

        // If status is not Pending, there is nothing to be done.
        if (!string.Equals(orderPart.Status.Text, OrderStatuses.Pending, StringComparison.OrdinalIgnoreCase))
        {
            await _notifier.InformationAsync(H["This Order is no longer pending."]);
            return this.RedirectToContentDisplay(orderId);
        }

        var singleCurrencyTotal = orderPart.LineItems.Select(item => item.LinePrice).Sum();
        if (singleCurrencyTotal.Value <= 0)
        {
            await _notifier.InformationAsync(H["This Order's line items have no cost, so there is nothing to be paid."]);
            return this.RedirectToContentDisplay(orderId);
        }

        var viewModel = new PaymentViewModel(orderPart, singleCurrencyTotal, singleCurrencyTotal);
        await viewModel.WithProviderDataAsync(_paymentProviders);

        return View(viewModel);
    }

    [Route("success/{orderId}")]
    public async Task<IActionResult> Success(string orderId)
    {
        if (await _contentManager.GetAsync(orderId) is not { } order) return NotFound();

        // Regular users should only see their own Orders, while users with the ManageOrders permission should be
        // able to see all Orders.
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOrders) && order.Author != User.Identity?.Name)
        {
            return NotFound();
        }

        order.DisplayText = T["Success"].Value; // This is only for display, intentionally not saved.

        return View(order);
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("checkout/free")]
    public async Task<IActionResult> CheckoutWithoutPayment(string? shoppingCartId) =>
        await _paymentService.CreatePendingOrderFromShoppingCartAsync(shoppingCartId, mustBeFree: true) is { } order
            ? await _paymentService.UpdateAndRedirectToFinishedOrderAsync(this, order, shoppingCartId)
            : NotFound();

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("checkout/callback/{paymentProviderName}/{orderId?}")]
    public async Task<IActionResult> Callback(string paymentProviderName, string? orderId, string? shoppingCartId)
    {
        if (string.IsNullOrWhiteSpace(paymentProviderName)) return NotFound();

        var order = string.IsNullOrEmpty(orderId)
            ? await _paymentService.CreatePendingOrderFromShoppingCartAsync(shoppingCartId, mustBeFree: false)
            : await _contentManager.GetAsync(orderId);
        if (order is null) return NotFound();

        var status = order.As<OrderPart>()?.Status?.Text ?? OrderStatuses.Pending.HtmlClassify();

        if (status == OrderStatuses.Ordered.HtmlClassify())
        {
            return this.RedirectToContentDisplay(order);
        }

        if (status == OrderStatuses.Pending.HtmlClassify())
        {
            foreach (var provider in _paymentProviders.WhereName(paymentProviderName))
            {
                if (await provider.UpdateAndRedirectToFinishedOrderAsync(this, order, shoppingCartId) is { } result)
                {
                    return result;
                }
            }

            return this.RedirectToContentDisplay(order);
        }

        await _notifier.ErrorAsync(H["The payment has failed, please try again."]);
        return RedirectToAction(nameof(Index));
    }
}
