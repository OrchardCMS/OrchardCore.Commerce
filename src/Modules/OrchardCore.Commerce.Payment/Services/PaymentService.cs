using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Activities;
using OrchardCore.Commerce.Constants;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Promotion.Extensions;
using OrchardCore.Commerce.Tax.Extensions;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Entities;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Settings;
using OrchardCore.Users;
using OrchardCore.Users.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Constants.ContentTypes;

namespace OrchardCore.Commerce.Services;

public class PaymentService : IPaymentService
{
    private readonly IFieldsOnlyDisplayManager _fieldsOnlyDisplayManager;
    private readonly IContentManager _contentManager;
    private readonly IShoppingCartHelpers _shoppingCartHelpers;
    private readonly ISiteService _siteService;
    private readonly UserManager<IUser> _userManager;
    private readonly IRegionService _regionService;
    private readonly Lazy<IUserService> _userServiceLazy;
    private readonly IShoppingCartPersistence _shoppingCartPersistence;
    private readonly IHttpContextAccessor _hca;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IContentItemDisplayManager _contentItemDisplayManager;

    // We need all of them.
#pragma warning disable S107 // Methods should not have too many parameters
    public PaymentService(
        IFieldsOnlyDisplayManager fieldsOnlyDisplayManager,
        IOrchardServices<PaymentService> services,
        IShoppingCartHelpers shoppingCartHelpers,
        IRegionService regionService,
        Lazy<IUserService> userServiceLazy,
        IShoppingCartPersistence shoppingCartPersistence,
        IUpdateModelAccessor updateModelAccessor,
        IContentItemDisplayManager contentItemDisplayManager)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        _fieldsOnlyDisplayManager = fieldsOnlyDisplayManager;
        _contentManager = services.ContentManager.Value;
        _shoppingCartHelpers = shoppingCartHelpers;
        _siteService = services.SiteService.Value;
        _userManager = services.UserManager.Value;
        _regionService = regionService;
        _userServiceLazy = userServiceLazy;
        _updateModelAccessor = updateModelAccessor;
        _contentItemDisplayManager = contentItemDisplayManager;
        _shoppingCartPersistence = shoppingCartPersistence;
        _hca = services.HttpContextAccessor.Value;
    }

    public async Task<ICheckoutViewModel> CreateCheckoutViewModelAsync(
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

        if (await _hca.HttpContext.GetUserDetailsAsync() is { } userDetails)
        {
            orderPart.Phone.Text = userDetails.PhoneNumber.Text;
            orderPart.VatNumber.Text = userDetails.VatNumber.Text;
            orderPart.IsCorporation.Value = userDetails.IsCorporation.Value;
        }

        var email = _hca.HttpContext?.User is { Identity.IsAuthenticated: true } user
            ? await _userManager.GetEmailAsync(await _userManager.GetUserAsync(user))
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

        var currency = total.Currency;
        var netTotal = new Amount(0, currency);
        var grossTotal = new Amount(0, currency);

        var lines = cart.Lines;
        foreach (var line in lines)
        {
            var additionalData = line.AdditionalData;
            var grossAmount = additionalData.GetGrossPrice();
            if (grossAmount.Value > 0)
            {
                grossTotal += grossAmount * line.Quantity;
            }

            var netAmount = additionalData.GetNetPrice();
            var netPrice = netAmount.Value > 0 ? netAmount : line.UnitPrice;
            netTotal += netPrice * line.Quantity;
        }

        var paymentProviderData = new Dictionary<string, object>();
        paymentProviderData["Stripe"] = new
        {
            StripePublishableKey = (await _siteService.GetSiteSettingsAsync()).As<StripeApiSettings>().PublishableKey,
            PaymentIntentClientSecret = await _stripePaymentService.CreateClientSecretAsync(total, cart),
        };

        return new CheckoutViewModel
        {
            ShoppingCartId = shoppingCartId,
            Regions = (await _regionService.GetAvailableRegionsAsync()).CreateSelectListOptions(),
            OrderPart = orderPart,
            SingleCurrencyTotal = total,
            NetTotal = netTotal,
            GrossTotal = grossTotal,
            UserEmail = email,
            CheckoutShapes = checkoutShapes,
            PaymentProviderData = paymentProviderData,
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
                var part = contentItem.TryGetValue(nameof(UserAddressesPart), out var partJson)
                    ? partJson.ToObject<UserAddressesPart>()!
                    : new UserAddressesPart();

                part.BillingAndShippingAddressesMatch.Value = isSame;
                contentItem[nameof(UserAddressesPart)] = JToken.FromObject(part);
                return contentItem;
            });
        }

        var currentShoppingCart = await _shoppingCartPersistence.RetrieveAsync();
        currentShoppingCart?.Items?.Clear();

        // Shopping cart ID is null by default currently.
        await _shoppingCartPersistence.StoreAsync(currentShoppingCart);

        // Set back to default, because a new payment intent should be created on the next checkout.
        _paymentIntentPersistence.Store(paymentIntentId: string.Empty);
    }

    public async Task<ContentItem> CreateNoPaymentOrderFromShoppingCartAsync(string shoppingCartId)
    {
        var currentShoppingCart = await _shoppingCartPersistence.RetrieveAsync(shoppingCartId);

        var order = await _contentManager.NewAsync(Order);
        if (await UpdateOrderWithDriversAsync(order))
        {
            return null;
        }

        var lineItems = await _shoppingCartHelpers.CreateOrderLineItemsAsync(currentShoppingCart);

        var cartViewModel = await _shoppingCartHelpers.CreateShoppingCartViewModelAsync(
            shoppingCartId: null,
            order.As<OrderPart>().ShippingAddress.Address,
            order.As<OrderPart>().BillingAddress.Address);

        if (!cartViewModel.Totals.Any() || cartViewModel.Totals.Any(total => total.Value > 0))
        {
            return null;
        }

        order.Alter<OrderPart>(orderPart =>
        {
            // Shopping cart
            orderPart.LineItems.SetItems(lineItems);

            orderPart.Status.Text = OrderStatuses.Pending.HtmlClassify();

            // Store the current applicable discount info, so they will be available in the future.
            orderPart.AdditionalData.SetDiscountsByProduct(cartViewModel
                .Lines
                .Where(line => line.AdditionalData.GetDiscounts().Any())
                .ToDictionary(
                    line => line.ProductSku,
                    line => line.AdditionalData.GetDiscounts()));
        });

        await _contentManager.CreateAsync(order);

        return order;
    }

    private async Task<bool> UpdateOrderWithDriversAsync(ContentItem order)
    {
        await _contentItemDisplayManager.UpdateEditorAsync(order, _updateModelAccessor.ModelUpdater, isNew: false);
        return _updateModelAccessor.ModelUpdater.GetModelErrorMessages().Any();
    }

    public async Task UpdateOrderToOrderedAsync(ContentItem order, Action<OrderPart> alterOrderPart = null)
    {
        ArgumentNullException.ThrowIfNull(order);

        order.Alter<OrderPart>(orderPart =>
        {
            alterOrderPart?.Invoke(orderPart);

            orderPart.Status = new TextField { ContentItem = order, Text = OrderStatuses.Ordered.HtmlClassify() };
        });

        await _workflowManagers.TriggerContentItemEventAsync<OrderCreatedEvent>(order);

        var currentShoppingCart = await _shoppingCartPersistence.RetrieveAsync();

        // Decrease inventories of purchased items.
        await _productInventoryService.UpdateInventoriesAsync(currentShoppingCart.Items);

        await _contentManager.UpdateAsync(order);
    }
}
