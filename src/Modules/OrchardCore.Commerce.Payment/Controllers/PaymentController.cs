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
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.MoneyDataType.Extensions;
using OrchardCore.Commerce.Payment;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrchardCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISession = YesSql.ISession;

namespace OrchardCore.Commerce.Controllers;

public class PaymentController : Controller
{
    private readonly ISiteService _siteService;
    private readonly ISession _session;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<PaymentController> _logger;
    private readonly IContentManager _contentManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IStringLocalizer T;
    private readonly IHtmlLocalizer<PaymentController> H;
    private readonly INotifier _notifier;
    private readonly IMoneyService _moneyService;
    private readonly IPaymentService _paymentService;

    public PaymentController(
        IOrchardServices<PaymentController> services,
        IUpdateModelAccessor updateModelAccessor,
        INotifier notifier,
        IMoneyService moneyService,
        IPaymentService paymentService)
    {
        _authorizationService = services.AuthorizationService.Value;
        _logger = services.Logger.Value;
        _contentManager = services.ContentManager.Value;
        _updateModelAccessor = updateModelAccessor;
        _session = services.Session.Value;
        T = services.StringLocalizer.Value;
        H = services.HtmlLocalizer.Value;
        _notifier = notifier;
        _moneyService = moneyService;
        _paymentService = paymentService;
        _siteService = services.SiteService.Value;
    }

    [Route("checkout")]
    public async Task<IActionResult> Index(string shoppingCartId = null)
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
    public async Task<IActionResult> Price(string shoppingCartId) =>
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

    [Route("checkout/validate")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Validate()
    {
        try
        {
            await _stripePaymentService.CreateOrUpdateOrderFromShoppingCartAsync(_updateModelAccessor);

            var errors = _updateModelAccessor.ModelUpdater.GetModelErrorMessages().ToList();
            return Json(new { Errors = errors });
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
        if (await _contentManager.GetAsync(orderId) is not { } order) return NotFound();

        if (string.IsNullOrEmpty(User.Identity?.Name))
        {
            return LocalRedirect($"~/Login?ReturnUrl=~/Contents/ContentItems/{orderId}");
        }

        var orderPart = order.As<OrderPart>();

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

        var paymentProviderData = new Dictionary<string, object>();
        var paymentIntent = await _stripePaymentService.CreatePaymentIntentAsync(singleCurrencyTotal);
        paymentProviderData["Stripe"] = new
        {
            StripePublishableKey = (await _siteService.GetSiteSettingsAsync()).As<StripeApiSettings>().PublishableKey,
            PaymentIntentClientSecret = paymentIntent.ClientSecret,
        };

        _session.Save(new OrderPayment
        {
            OrderId = order.ContentItemId,
            PaymentIntentId = paymentIntent.Id,
        });

        return View(new PaymentViewModel
        {
            SingleCurrencyTotal = singleCurrencyTotal,
            NetTotal = singleCurrencyTotal,
            PaymentProviderData = paymentProviderData,
            OrderPart = orderPart,
        });
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
    public async Task<IActionResult> CheckoutWithoutPayment(string shoppingCartId)
    {
        if (await _paymentService.CreateNoPaymentOrderFromShoppingCartAsync(shoppingCartId) is not { } order)
        {
            return NotFound();
        }

        await _paymentService.UpdateOrderToOrderedAsync(order);
        await _paymentService.FinalModificationOfOrderAsync(order);

        return RedirectToAction(nameof(Success), new { orderId = order.ContentItem.ContentItemId });
    }
}