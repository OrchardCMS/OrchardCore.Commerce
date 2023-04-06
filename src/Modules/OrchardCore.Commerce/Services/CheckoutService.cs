using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Activities;
using OrchardCore.Commerce.Controllers;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Users;
using OrchardCore.Workflows.Services;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Constants.ContentTypes;

namespace OrchardCore.Commerce.Services;

public class CheckoutService : ICheckoutService
{
    private readonly IEnumerable<IWorkflowManager> _workflowManagers;
    private readonly IAuthorizationService _authorizationService;
    private readonly IStripePaymentService _stripePaymentService;
    private readonly IFieldsOnlyDisplayManager _fieldsOnlyDisplayManager;
    private readonly ILogger<CheckoutService> _logger;
    private readonly IContentManager _contentManager;
    private readonly IShoppingCartHelpers _shoppingCartHelpers;
    private readonly ISiteService _siteService;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly UserManager<IUser> _userManager;
    private readonly IStringLocalizer T;
    private readonly IHtmlLocalizer<CheckoutService> H;
    private readonly IRegionService _regionService;
    private readonly Lazy<IUserService> _userServiceLazy;
    private readonly IPaymentIntentPersistence _paymentIntentPersistence;
    private readonly IShoppingCartPersistence _shoppingCartPersistence;
    private readonly INotifier _notifier;
    private readonly IMoneyService _moneyService;
    private readonly ICheckoutService _checkoutService;
    private readonly IHttpContextAccessor _hca;

    // We need all of them.
#pragma warning disable S107 // Methods should not have too many parameters
    public CheckoutService(
        IStripePaymentService stripePaymentService,
        IFieldsOnlyDisplayManager fieldsOnlyDisplayManager,
        IOrchardServices<CheckoutService> services,
        IShoppingCartHelpers shoppingCartHelpers,
        ISiteService siteService,
        IUpdateModelAccessor updateModelAccessor,
        IRegionService regionService,
        Lazy<IUserService> userServiceLazy,
        IEnumerable<IWorkflowManager> workflowManagers,
        IPaymentIntentPersistence paymentIntentPersistence,
        IShoppingCartPersistence shoppingCartPersistence,
        INotifier notifier,
        IMoneyService moneyService,
        ICheckoutService checkoutService)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        _authorizationService = services.AuthorizationService.Value;
        _stripePaymentService = stripePaymentService;
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
        H = services.HtmlLocalizer.Value;
        _paymentIntentPersistence = paymentIntentPersistence;
        _shoppingCartPersistence = shoppingCartPersistence;
        _notifier = notifier;
        _moneyService = moneyService;
        _checkoutService = checkoutService;
        _hca = services.HttpContextAccessor.Value;
    }

    public async Task<CheckoutViewModel> CreateCheckoutViewModelAsync(
        string shoppingCartId,
        Action<OrderPart> updateOrderPart = null)
    {
        var orderPart = new OrderPart();

        if (await _hca.HttpContext.GetUserAddressAsync() is { } userAddresses)
        {
            orderPart.BillingAddress.Address = userAddresses.BillingAddress.Address;
            orderPart.ShippingAddress.Address = userAddresses.ShippingAddress.Address;
            orderPart.BillingAndShippingAddressesMatch.Value = userAddresses.BillingAndShippingAddressesMatch.Value;
        }

        var email = _hca.HttpContext != null && _hca.HttpContext.User.Identity?.IsAuthenticated == true
            ? await _userManager.GetEmailAsync(await _userManager.GetUserAsync(_hca.HttpContext.User))
            : string.Empty;

        orderPart.Email.Text = email;
        orderPart.ShippingAddress.UserAddressToSave = nameof(orderPart.ShippingAddress);
        orderPart.BillingAddress.UserAddressToSave = nameof(orderPart.BillingAddress);

        updateOrderPart?.Invoke(orderPart);

        var cart = await _shoppingCartHelpers.CreateShoppingCartViewModelAsync(
            shoppingCartId,
            orderPart.ShippingAddress.Address,
            orderPart.BillingAddress.Address);
        if (cart?.Totals.Single() is not { } total) return null;

        var checkoutShapes = (await _fieldsOnlyDisplayManager.DisplayFieldsAsync(
                await _contentManager.NewAsync(Order),
                "Checkout"))
            .ToList();

        var stripeApiSettings = (await _siteService.GetSiteSettingsAsync()).As<StripeApiSettings>();
        var initPaymentIntent = new PaymentIntent();
        if (!string.IsNullOrEmpty(stripeApiSettings.PublishableKey) &&
            !string.IsNullOrEmpty(stripeApiSettings.SecretKey))
        {
            var paymentIntentId = _paymentIntentPersistence.Retrieve();
            initPaymentIntent = await _stripePaymentService.InitializePaymentIntentAsync(paymentIntentId);
        }

        return new CheckoutViewModel
        {
            ShoppingCartId = shoppingCartId,
            Regions = (await _regionService.GetAvailableRegionsAsync()).CreateSelectListOptions(),
            OrderPart = orderPart,
            SingleCurrencyTotal = total,
            StripePublishableKey = (await _siteService.GetSiteSettingsAsync()).As<StripeApiSettings>().PublishableKey,
            UserEmail = email,
            CheckoutShapes = checkoutShapes,
            PaymentIntentClientSecret = initPaymentIntent?.ClientSecret,
        };
    }

    public async Task FinalModificationOfOrderAsync(ContentItem order)
    {
        // Saving addresses.
        var userService = _userServiceLazy.Value;
        var orderPart = order.As<OrderPart>();

        if (_hca.HttpContext != null && await userService.GetFullUserAsync(_hca.HttpContext.User) is { } user)
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

        order.DisplayText = T["Order {0}", order.As<OrderPart>().OrderId.Text];

        await _contentManager.UpdateAsync(order);

        if (_workflowManagers.FirstOrDefault() is { } workflowManager)
        {
            await workflowManager.TriggerEventAsync(nameof(OrderCreatedEvent), order, "Order-" + order.ContentItemId);
        }

        var currentShoppingCart = await _shoppingCartPersistence.RetrieveAsync();
        currentShoppingCart?.Items?.Clear();

        // Shopping cart ID is null by default currently.
        await _shoppingCartPersistence.StoreAsync(currentShoppingCart);

        // Set back to default, because a new payment intent should be created on the next checkout.
        _paymentIntentPersistence.Store(paymentIntentId: string.Empty);
    }
}
