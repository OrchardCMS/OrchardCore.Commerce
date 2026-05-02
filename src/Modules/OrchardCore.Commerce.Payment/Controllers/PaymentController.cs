using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Abstractions.Constants;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.MoneyDataType.Extensions;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Controllers;

public class PaymentController : PaymentBaseController
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentManager _contentManager;
    private readonly IStringLocalizer T;
    private readonly IHtmlLocalizer<PaymentController> H;
    private readonly INotifier _notifier;
    private readonly IEnumerable<IPaymentProvider> _paymentProviders;
    private readonly IPaymentService _paymentService;
    public PaymentController(
        IOrchardServices<PaymentController> services,
        INotifier notifier,
        IEnumerable<IPaymentProvider> paymentProviders,
        IPaymentService paymentService)
        : base(notifier, services.Logger.Value)
    {
        _authorizationService = services.AuthorizationService.Value;
        _contentManager = services.ContentManager.Value;
        T = services.StringLocalizer.Value;
        H = services.HtmlLocalizer.Value;
        _notifier = notifier;
        _paymentProviders = paymentProviders;
        _paymentService = paymentService;
    }

    [HttpGet("checkout")]
    public async Task<IActionResult> Index([FromQuery] string? shoppingCartId)
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

        return View(checkoutViewModel);
    }

    [Route("checkout/price")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Price([FromQuery] string? shoppingCartId) =>
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

    [Route("checkout/validate/{providerName}/{paymentId?}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Validate(string providerName, string paymentId, [FromQuery] string? shoppingCartId = null)
    {
        if (string.IsNullOrEmpty(providerName)) return NotFound();

        var errors = await _paymentService.ValidateErrorsAsync(providerName, shoppingCartId, paymentId);
        return Json(new { Errors = errors });
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
        await viewModel.WithProviderDataAsync(_paymentProviders, isPaymentRequest: true);

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
    public async Task<IActionResult> CheckoutWithoutPayment([FromQuery] string? shoppingCartId, [FromQuery] bool mustBeFree)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _paymentService.CheckoutWithoutPaymentAsync(shoppingCartId, mustBeFree);
        return await ProduceActionResultAsync(result);
    }

    [HttpGet]
    [Route("checkout/callback/{paymentProviderName}/{orderId?}")]
    public Task<IActionResult> CallbackGet(string paymentProviderName, string? orderId, [FromQuery] string? shoppingCartId) =>
        Callback(paymentProviderName, orderId, shoppingCartId);

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("checkout/callback/{paymentProviderName}/{orderId?}")]
    public async Task<IActionResult> Callback(string paymentProviderName, string? orderId, [FromQuery] string? shoppingCartId)
    {
        var result = await _paymentService.CallBackAsync(paymentProviderName, orderId, shoppingCartId);
        return await ProduceActionResultAsync(result);
    }

    [HttpGet]
    [Route("checkout/wait")]
    public IActionResult Wait([FromQuery] string returnUrl) => View(new CheckoutWaitViewModel(returnUrl));
}
