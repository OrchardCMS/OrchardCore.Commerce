using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Constants;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Entities;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Settings;
using OrchardCore.Users;
using Stripe;
using System;
using System.Threading.Tasks;
using CommerceContentTypes = OrchardCore.Commerce.Constants.ContentTypes;

namespace OrchardCore.Commerce.Controllers;

public class PaymentController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ICardPaymentService _cardPaymentService;
    private readonly IContentItemDisplayManager _contentItemDisplayManager;
    private readonly ILogger _logger;
    private readonly IContentManager _contentManager;
    private readonly IShoppingCartHelpers _shoppingCartHelpers;
    private readonly ISiteService _siteService;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly UserManager<IUser> _userManager;
    private readonly IStringLocalizer T;

    public PaymentController(
        ICardPaymentService cardPaymentService,
        IContentItemDisplayManager contentItemDisplayManager,
        IOrchardServices<PaymentController> services,
        IShoppingCartHelpers shoppingCartHelpers,
        ISiteService siteService,
        IUpdateModelAccessor updateModelAccessor)
    {
        _authorizationService = services.AuthorizationService.Value;
        _cardPaymentService = cardPaymentService;
        _contentItemDisplayManager = contentItemDisplayManager;
        _logger = services.Logger.Value;
        _contentManager = services.ContentManager.Value;
        _shoppingCartHelpers = shoppingCartHelpers;
        _siteService = siteService;
        _updateModelAccessor = updateModelAccessor;
        _userManager = services.UserManager.Value;
        T = services.StringLocalizer.Value;
    }

    [Route("checkout")]
    public async Task<IActionResult> Index()
    {
        var isAuthenticated = User.Identity?.IsAuthenticated == true;
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.Checkout))
        {
            return isAuthenticated ? Forbid() : LocalRedirect("~/Login?ReturnUrl=~/checkout");
        }

        if (await _shoppingCartHelpers.CalculateSingleCurrencyTotalAsync() is not { } total)
        {
            return RedirectToAction(nameof(ShoppingCartController.Empty), nameof(ShoppingCartController));
        }

        var order = await _contentManager.NewAsync(CommerceContentTypes.Order);
        var editor = await _contentItemDisplayManager.BuildEditorAsync(order, _updateModelAccessor.ModelUpdater, isNew: true);
        var email = isAuthenticated ? await _userManager.GetEmailAsync(await _userManager.GetUserAsync(User)) : null;

        return View(new CheckoutViewModel
        {
            NewOrderEditor = editor,
            SingleCurrencyTotal = total,
            StripePublishableKey = (await _siteService.GetSiteSettingsAsync()).As<StripeApiSettings>().PublishableKey,
            UserEmail = email ?? string.Empty,
        });
    }

    [Route("success/{orderId}")]
    public async Task<IActionResult> Success(string orderId)
    {
        if (await _contentManager.GetAsync(orderId) is not { } order) return NotFound();

        order.DisplayText = T["Success"].Value; // This is only for display, intentionally not saved.
        return View(order);
    }

    [Route("success/{orderId}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SuccessPost(string orderId)
    {
        if (await _contentManager.GetAsync(orderId) is not { } order) return NotFound();

        await _contentItemDisplayManager.UpdateEditorAsync(order, _updateModelAccessor.ModelUpdater, isNew: false);
        order.Alter<OrderPart>(part => part.Status =
            new TextField { ContentItem = order, Text = OrderStatuses.Ordered.HtmlClassify() });
        order.DisplayText = T["Order {0}", order.As<OrderPart>().OrderId.Text];
        await _contentManager.UpdateAsync(order);

        return Redirect($"~/success/{order.ContentItemId}");
    }

    [Route("pay")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pay(string paymentMethodId, string paymentIntentId)
    {
        PaymentIntent paymentIntent;

        try
        {
            paymentIntent = await _cardPaymentService.CreatePaymentAsync(paymentMethodId, paymentIntentId);
        }
        catch (StripeException exception)
        {
            return Json(new { error = exception.StripeError.Message });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Payment processing failed.");
            var message = T["An error has occurred while processing the payment. Please verify and try again."];
            return Json(new { error = message.Value });
        }

        return await GeneratePaymentResponseAsync(paymentIntent);
    }

    private async Task<IActionResult> GeneratePaymentResponseAsync(PaymentIntent paymentIntent)
    {
        if (paymentIntent.Status == "requires_action" &&
            paymentIntent.NextAction.Type == "use_stripe_sdk")
        {
            // Tell the client to handle the action.
            return Json(new
            {
                requires_action = true,
                payment_intent_client_secret = paymentIntent.ClientSecret,
            });
        }

        if (paymentIntent.Status == "succeeded")
        {
            // The payment didn’t need any additional actions and completed!
            // Create the order content item.
            var order = await _cardPaymentService.CreateOrderFromShoppingCartAsync(paymentIntent);

            return Json(new { Success = true, OrderContentItemId = order.ContentItemId });
        }

        // Invalid status.
        return StatusCode(StatusCodes.Status500InternalServerError, new { error = T["Invalid PaymentIntent status"].Value });
    }
}
