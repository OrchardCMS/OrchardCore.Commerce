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
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISession = YesSql.ISession;

namespace OrchardCore.Commerce.Controllers;

public class PaymentController : Controller
{
    private const string FormValidationExceptionMessage = "An exception has occurred during checkout form validation.";

    private readonly ISiteService _siteService;
    private readonly ISession _session;
    private readonly IAuthorizationService _authorizationService;
    private readonly IStripePaymentService _stripePaymentService;
    private readonly ILogger<PaymentController> _logger;
    private readonly IContentManager _contentManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IStringLocalizer T;
    private readonly IHtmlLocalizer<PaymentController> H;
    private readonly IPaymentIntentPersistence _paymentIntentPersistence;
    private readonly INotifier _notifier;
    private readonly IMoneyService _moneyService;
    private readonly IPaymentService _paymentService;

    public PaymentController(
        IStripePaymentService stripePaymentService,
        IOrchardServices<PaymentController> services,
        IUpdateModelAccessor updateModelAccessor,
        IPaymentIntentPersistence paymentIntentPersistence,
        INotifier notifier,
        IMoneyService moneyService,
        IPaymentService paymentService)
    {
        _authorizationService = services.AuthorizationService.Value;
        _stripePaymentService = stripePaymentService;
        _logger = services.Logger.Value;
        _contentManager = services.ContentManager.Value;
        _updateModelAccessor = updateModelAccessor;
        _session = services.Session.Value;
        T = services.StringLocalizer.Value;
        H = services.HtmlLocalizer.Value;
        _paymentIntentPersistence = paymentIntentPersistence;
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
            return RedirectToAction(
                nameof(ShoppingCartController.Empty),
                typeof(ShoppingCartController).ControllerName());
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
            var paymentIntent = _paymentIntentPersistence.Retrieve();
            var paymentIntentInstance = await _stripePaymentService.GetPaymentIntentAsync(paymentIntent);
            await _stripePaymentService.CreateOrUpdateOrderFromShoppingCartAsync(paymentIntentInstance, _updateModelAccessor);

            var errors = _updateModelAccessor.ModelUpdater.GetModelErrorMessages().ToList();
            return Json(new { Errors = errors });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, FormValidationExceptionMessage);

            var errorMessage = HttpContext.IsDevelopmentAndLocalhost()
                ? exception.ToString()
                : FormValidationExceptionMessage;

            return Json(new { Errors = new[] { errorMessage } });
        }
    }

    [Route("PaymentRequest/{orderId}")]
    public async Task<IActionResult> PaymentRequest(string orderId)
    {
        if (await _contentManager.GetAsync(orderId) is not { } order) return NotFound();

        // Users should only see their own Orders.
        if (order.Author != User.Identity.Name)
        {
            return NotFound();
        }

        var orderPart = order.As<OrderPart>();

        // If status is not Pending or there are no line items, there is nothing to be done.
        if (!string.Equals(orderPart.Status.Text, OrderStatuses.Pending, StringComparison.OrdinalIgnoreCase) ||
            !orderPart.LineItems.Any())
        {
            return NotFound();
        }

        var currency = orderPart.LineItems[0].LinePrice.Currency;
        var singleCurrencyTotal = new Amount(0, currency);
        foreach (var item in orderPart.LineItems)
        {
            singleCurrencyTotal += item.LinePrice;
        }

        if (singleCurrencyTotal.Value <= 0)
        {
            return NotFound();
        }

        var stripeApiSettings = (await _siteService.GetSiteSettingsAsync()).As<StripeApiSettings>();
        var paymentAmount = _stripePaymentService.GetPaymentAmount(singleCurrencyTotal.Value, currency.CurrencyIsoCode);
        var paymentIntent = await _stripePaymentService.CreatePaymentIntentAsync(paymentAmount, singleCurrencyTotal);

        _session.Save(new OrderPayment
        {
            OrderId = order.ContentItemId,
            PaymentIntentId = paymentIntent.Id,
        });

        return View(new
        {
            SingleCurrencyTotal = singleCurrencyTotal,
            NetTotal = singleCurrencyTotal,
            GrossTotal = new Amount(0, currency),
            StripePublishableKey = stripeApiSettings.PublishableKey,
            PaymentIntentClientSecret = paymentIntent.ClientSecret,
            OrderPart = orderPart,
        });
    }

    [Route("success/{orderId}")]
    public async Task<IActionResult> Success(string orderId)
    {
        if (await _contentManager.GetAsync(orderId) is not { } order) return NotFound();

        // Regular users should only see their own Orders, while users with the ManageOrders permission should be
        // able to see all Orders.
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOrders) && order.Author != User.Identity.Name)
        {
            return NotFound();
        }

        order.DisplayText = T["Success"].Value; // This is only for display, intentionally not saved.

        return View(order);
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckoutWithoutPayment()
    {
        if (await _paymentService.CreateNoPaymentOrderFromShoppingCartAsync() is not { } order)
        {
            return NotFound();
        }

        await _stripePaymentService.UpdateOrderToOrderedAsync(orderItem: order);
        await _paymentService.FinalModificationOfOrderAsync(order);

        return RedirectToAction(nameof(Success), new { orderId = order.ContentItem.ContentItemId });
    }

    [AllowAnonymous]
    [HttpGet("checkout/middleware")]
    public async Task<IActionResult> PaymentConfirmationMiddleware([FromQuery(Name = "payment_intent")] string paymentIntent = null)
    {
        // If it is null it means the session was not loaded yet and a redirect is needed.
        if (string.IsNullOrEmpty(_paymentIntentPersistence.Retrieve()))
        {
            return View();
        }

        var fetchedPaymentIntent = await _stripePaymentService.GetPaymentIntentAsync(paymentIntent);
        var orderId = (await _stripePaymentService.GetOrderPaymentByPaymentIntentIdAsync(paymentIntent))?.OrderId;

        var order = await _contentManager.GetAsync(orderId);
        var status = order?.As<OrderPart>()?.Status?.Text;
        var succeeded = fetchedPaymentIntent.Status == PaymentIntentStatuses.Succeeded;
        var finished = succeeded &&
                       order != null &&
                       status == OrderStatuses.Ordered.HtmlClassify();

        if (succeeded && status == OrderStatuses.Pending.HtmlClassify())
        {
            await _stripePaymentService.UpdateOrderToOrderedAsync(fetchedPaymentIntent);
            finished = true;
        }

        if (finished)
        {
            await _paymentService.FinalModificationOfOrderAsync(order);

            return RedirectToAction(nameof(Success), new { orderId });
        }

        var errorMessage = H["The payment failed, please try again."];
        if (status == OrderStatuses.PaymentFailed.HtmlClassify())
        {
            await _notifier.ErrorAsync(errorMessage);
            return RedirectToAction(nameof(Index));
        }

        order.Alter<StripePaymentPart>(part => part.RetryCounter++);
        await _contentManager.UpdateAsync(order);

        if (order.As<StripePaymentPart>().RetryCounter <= 10)
        {
            return View();
        }

        // Delete payment intent from session, to create a new one.
        _paymentIntentPersistence.Store(string.Empty);
        await _notifier.ErrorAsync(errorMessage);
        return RedirectToAction(nameof(Index));
    }
}
