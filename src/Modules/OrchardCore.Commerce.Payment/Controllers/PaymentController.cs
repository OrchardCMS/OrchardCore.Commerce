using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.Constants;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.MoneyDataType.Extensions;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.Constants;
using OrchardCore.Commerce.Payment.ViewModels;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Mvc.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrontendException = Lombiq.HelpfulLibraries.AspNetCore.Exceptions.FrontendException;

namespace OrchardCore.Commerce.Payment.Controllers;

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
    private readonly IRegionService _regionService;

    public PaymentController(
        IOrchardServices<PaymentController> services,
        IUpdateModelAccessor updateModelAccessor,
        INotifier notifier,
        IMoneyService moneyService,
        IEnumerable<IPaymentProvider> paymentProviders,
        IPaymentService paymentService,
        IRegionService regionService)
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
        _regionService = regionService;
    }

    [HttpGet("checkout")]
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

        if (checkoutViewModel.IsInvalid)
        {
            await _notifier.ErrorAsync(H["Checkout unavailable — invalid item in cart."]);
            return LocalRedirect("~/cart");
        }

        foreach (dynamic shape in checkoutViewModel.CheckoutShapes) shape.ViewModel = checkoutViewModel;

        checkoutViewModel.Provinces.AddRange(await _regionService.GetAllProvincesAsync());

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

            var total = await _paymentService.GetTotalAsync(shoppingCartId);

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
            return Json(new { Errors = exception.HtmlMessages });
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

    [HttpGet("checkout/paymentrequest/{orderId}")]
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

        if (!viewModel.PaymentProviderData.Any())
        {
            await _notifier.WarningAsync(new HtmlString(" ").Join(
                H["There are no applicable payment providers for this site."],
                H["Please make sure there is at least one enabled and properly configured."]));
        }

        return View(viewModel);
    }

    [HttpGet("success/{orderId}")]
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

    [HttpGet]
    [Route("checkout/callback/{paymentProviderName}/{orderId?}")]
    public Task<IActionResult> CallbackGet(string paymentProviderName, string? orderId, string? shoppingCartId) =>
        Callback(paymentProviderName, orderId, shoppingCartId);

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("checkout/callback/{paymentProviderName}/{orderId?}")]
    public async Task<IActionResult> Callback(string paymentProviderName, string? orderId, string? shoppingCartId)
    {
        if (string.IsNullOrWhiteSpace(paymentProviderName)) return NotFound();

        var order = string.IsNullOrEmpty(orderId)
            ? await _paymentService.CreatePendingOrderFromShoppingCartAsync(shoppingCartId)
            : await _contentManager.GetAsync(orderId);
        if (order is null) return NotFound();

        var status = order.As<OrderPart>()?.Status?.Text ?? OrderStatusCodes.Pending;

        if (status is not OrderStatusCodes.Pending and not OrderStatusCodes.PaymentFailed)
        {
            return this.RedirectToContentDisplay(order);
        }

        foreach (var provider in _paymentProviders.WhereName(paymentProviderName))
        {
            if (await provider.UpdateAndRedirectToFinishedOrderAsync(this, order, shoppingCartId) is { } result)
            {
                return result;
            }
        }

        await _notifier.ErrorAsync(H["The payment has failed, please try again."]);
        return RedirectToAction(nameof(Index));
    }

    [Route("checkout/wait")]
    public IActionResult Wait(string returnUrl) => View(new CheckoutWaitViewModel(returnUrl));

    public static IActionResult RedirectToWait(Controller controller, string? returnUrl = null) =>
        controller.RedirectToAction(
            nameof(Wait),
            typeof(PaymentController).ControllerName(),
            new
            {
                area = FeatureIds.Payment,
                returnUrl = string.IsNullOrEmpty(returnUrl)
                    ? controller.HttpContext.Request.GetDisplayUrl()
                    : returnUrl,
            });
}
