using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Activities;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.Constants;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Entities;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Settings;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrchardCore.Workflows.Services;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Constants.ContentTypes;

namespace OrchardCore.Commerce.Controllers;

public class PaymentController : Controller
{
    private const string FormValidationExceptionMessage = "An exception has occurred during checkout form validation.";

    private readonly IEnumerable<IWorkflowManager> _workflowManagers;
    private readonly IAuthorizationService _authorizationService;
    private readonly IPaymentService _paymentService;
    private readonly IContentItemDisplayManager _contentItemDisplayManager;
    private readonly IFieldsOnlyDisplayManager _fieldsOnlyDisplayManager;
    private readonly ILogger _logger;
    private readonly IContentManager _contentManager;
    private readonly IShoppingCartHelpers _shoppingCartHelpers;
    private readonly ISiteService _siteService;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly UserManager<IUser> _userManager;
    private readonly IStringLocalizer T;
    private readonly IRegionService _regionService;
    private readonly Lazy<IUserService> _userServiceLazy;
    private readonly IPaymentIntentPersistence _paymentIntentPersistence;

    // We need all of them.
#pragma warning disable S107 // Methods should not have too many parameters
    public PaymentController(
        IPaymentService paymentService,
        IContentItemDisplayManager contentItemDisplayManager,
        IFieldsOnlyDisplayManager fieldsOnlyDisplayManager,
        IOrchardServices<PaymentController> services,
        IShoppingCartHelpers shoppingCartHelpers,
        ISiteService siteService,
        IUpdateModelAccessor updateModelAccessor,
        IRegionService regionService,
        Lazy<IUserService> userServiceLazy,
        IEnumerable<IWorkflowManager> workflowManagers,
        IPaymentIntentPersistence paymentIntentPersistence)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        _authorizationService = services.AuthorizationService.Value;
        _paymentService = paymentService;
        _contentItemDisplayManager = contentItemDisplayManager;
        _fieldsOnlyDisplayManager = fieldsOnlyDisplayManager;
        _logger = services.Logger.Value;
        _contentManager = services.ContentManager.Value;
        _shoppingCartHelpers = shoppingCartHelpers;
        _siteService = siteService;
        _updateModelAccessor = updateModelAccessor;
        _userManager = services.UserManager.Value;
        _regionService = regionService;
        _userServiceLazy = userServiceLazy;
        _workflowManagers = workflowManagers;
        T = services.StringLocalizer.Value;
        _paymentIntentPersistence = paymentIntentPersistence;
    }

    [Route("checkout")]
    public async Task<IActionResult> Index(string shoppingCartId = null, string paymentIntent = null)
    {
        var isAuthenticated = User.Identity?.IsAuthenticated == true;
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.Checkout))
        {
            return isAuthenticated ? Forbid() : LocalRedirect("~/Login?ReturnUrl=~/checkout");
        }

        if (await _shoppingCartHelpers.CreateShoppingCartViewModelAsync(shoppingCartId) is not { } cart)
        {
            return RedirectToAction(
                nameof(ShoppingCartController.Empty),
                typeof(ShoppingCartController).ControllerName());
        }

        var orderPart = new OrderPart();

        if (await _userManager.GetUserAsync(User) is User user &&
            user.As<ContentItem>(UserAddresses)?.As<UserAddressesPart>() is { } userAddresses)
        {
            orderPart.BillingAddress.Address = userAddresses.BillingAddress.Address;
            orderPart.ShippingAddress.Address = userAddresses.ShippingAddress.Address;
            orderPart.BillingAndShippingAddressesMatch.Value = userAddresses.BillingAndShippingAddressesMatch.Value;
        }

        var email = isAuthenticated
            ? await _userManager.GetEmailAsync(await _userManager.GetUserAsync(User))
            : string.Empty;

        orderPart.Email.Text = email;
        orderPart.ShippingAddress.UserAddressToSave = nameof(orderPart.ShippingAddress);
        orderPart.BillingAddress.UserAddressToSave = nameof(orderPart.BillingAddress);

        var total = cart.Totals.Single();

        var checkoutShapes = (await _fieldsOnlyDisplayManager.DisplayFieldsAsync(
                await _contentManager.NewAsync(Order),
                "Checkout"))
            .ToList();

        if (string.IsNullOrEmpty(paymentIntent))
        {
            paymentIntent = _paymentIntentPersistence.Retrieve();
        }

        var initPaymentIntent = await _paymentService.InitializePaymentIntentAsync(paymentIntent);

        var checkoutViewModel = new CheckoutViewModel
        {
            Regions = (await _regionService.GetAvailableRegionsAsync()).CreateSelectListOptions(),
            OrderPart = orderPart,
            SingleCurrencyTotal = total,
            StripePublishableKey = (await _siteService.GetSiteSettingsAsync()).As<StripeApiSettings>().PublishableKey,
            UserEmail = email,
            CheckoutShapes = checkoutShapes,
            PaymentIntent = initPaymentIntent,
            EnableInputs = initPaymentIntent.Status == "requires_payment_method",
        };

        foreach (dynamic shape in checkoutShapes) shape.ViewModel = checkoutViewModel;

        checkoutViewModel.Provinces.AddRange(Regions.Provinces);

        return View(checkoutViewModel);
    }

    [Route("checkout/validate")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Validate()
    {
        try
        {
            var order = await _contentManager.NewAsync(Order);
            await _contentItemDisplayManager.UpdateEditorAsync(order, _updateModelAccessor.ModelUpdater, isNew: false);
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
        if (!_updateModelAccessor.ModelUpdater.ModelState.IsValid)
        {
            var errors = _updateModelAccessor.ModelUpdater.GetModelErrorMessages();
            _logger.LogError(
                "The payment has been successful, but the order is invalid. Validation errors: {ValidationErrors}",
                string.Join(", ", errors));

            return Forbid();
        }

        // Saving addresses.
        var userService = _userServiceLazy.Value;
        var orderPart = order.As<OrderPart>();

        if (await userService.GetFullUserAsync(User) is { } user)
        {
            var isSame = orderPart.BillingAndShippingAddressesMatch.Value;

            await userService.AlterUserSettingAsync(user, UserAddresses, contentItem =>
            {
                var part = contentItem.ContainsKey(nameof(UserAddressesPart))
                    ? contentItem[nameof(UserAddressesPart)].ToObject<UserAddressesPart>()!
                    : new UserAddressesPart();

                part.BillingAndShippingAddressesMatch.Value = isSame;
                contentItem[nameof(UserAddressesPart)] = JToken.FromObject(part);
                return contentItem;
            });
        }

        order.Alter<OrderPart>(part => part.Status =
            new TextField { ContentItem = order, Text = OrderStatuses.Ordered.HtmlClassify() });

        order.DisplayText = T["Order {0}", order.As<OrderPart>().OrderId.Text];

        await _contentManager.UpdateAsync(order);

        if (_workflowManagers.FirstOrDefault() is { } workflowManager)
        {
            await workflowManager.TriggerEventAsync(nameof(OrderCreatedEvent), order, "Order-" + order.ContentItemId);
        }

        return Redirect($"~/success/{order.ContentItemId}");
    }

    [Route("pay")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPayment(string paymentId)
    {
        PaymentIntent paymentIntent;

        try
        {
            paymentIntent = await _paymentService.GetPaymentIntentAsync(paymentId);
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
        if (paymentIntent.Status == PaymentIntentStatuses.RequiresAction &&
            paymentIntent.NextAction.Type == "use_stripe_sdk")
        {
            // Tell the client to handle the action.
            return Json(new { requires_action = true, payment_intent_client_secret = paymentIntent.ClientSecret, });
        }

        if (paymentIntent.Status == PaymentIntentStatuses.RequiresConfirmation)
        {
            return Json(new { requires_action = false });
        }

        if (paymentIntent.Status == PaymentIntentStatuses.RequiresPaymentMethod)
        {
            return Json(new
            {
                requires_payment_method = true,
                payment_intent_client_secret = paymentIntent.ClientSecret,
                error = T["An error has occurred while processing the payment. Please try again."].Value,
            });
        }

        if (paymentIntent.Status == PaymentIntentStatuses.Succeeded)
        {
            // The payment didn’t need any additional actions and completed!
            // Create the order content item.
            var order = await _paymentService.CreateOrderFromShoppingCartAsync(paymentIntent);

            return Json(new { Success = true, OrderContentItemId = order.ContentItemId });
        }

        // Invalid status.
        return StatusCode(StatusCodes.Status500InternalServerError,
            new { error = T["Invalid PaymentIntent status"].Value });
    }
}
