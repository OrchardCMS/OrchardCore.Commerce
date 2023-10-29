using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Constants;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Tax.Extensions;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class PaymentService : IPaymentService
{
    private readonly IFieldsOnlyDisplayManager _fieldsOnlyDisplayManager;
    private readonly IEnumerable<IOrderEvents> _orderEvents;
    private readonly IContentManager _contentManager;
    private readonly IShoppingCartHelpers _shoppingCartHelpers;
    private readonly UserManager<IUser> _userManager;
    private readonly IRegionService _regionService;
    private readonly IHttpContextAccessor _hca;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IContentItemDisplayManager _contentItemDisplayManager;
    private readonly IEnumerable<IPaymentProvider> _paymentProviders;

    // We need all of them.
#pragma warning disable S107 // Methods should not have too many parameters
    public PaymentService(
        IFieldsOnlyDisplayManager fieldsOnlyDisplayManager,
        IEnumerable<IOrderEvents> orderEvents,
        IOrchardServices<PaymentService> services,
        IShoppingCartHelpers shoppingCartHelpers,
        IRegionService regionService,
        IUpdateModelAccessor updateModelAccessor,
        IContentItemDisplayManager contentItemDisplayManager,
        IEnumerable<IPaymentProvider> paymentProviders)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        _fieldsOnlyDisplayManager = fieldsOnlyDisplayManager;
        _orderEvents = orderEvents;
        _contentManager = services.ContentManager.Value;
        _shoppingCartHelpers = shoppingCartHelpers;
        _userManager = services.UserManager.Value;
        _regionService = regionService;
        _updateModelAccessor = updateModelAccessor;
        _contentItemDisplayManager = contentItemDisplayManager;
        _paymentProviders = paymentProviders;
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
                await _contentManager.NewAsync("Order"),
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

        var viewModel = new CheckoutViewModel
        {
            ShoppingCartId = shoppingCartId,
            Regions = (await _regionService.GetAvailableRegionsAsync()).CreateSelectListOptions(),
            OrderPart = orderPart,
            SingleCurrencyTotal = total,
            NetTotal = netTotal,
            GrossTotal = grossTotal,
            UserEmail = email,
            CheckoutShapes = checkoutShapes,
        };

        await viewModel.WithProviderDataAsync(_paymentProviders);
        return viewModel;
    }

    public async Task FinalModificationOfOrderAsync(ContentItem order, string shoppingCartId, string paymentProviderName)
    {
        await _orderEvents.AwaitEachAsync(orderEvent =>
            orderEvent.FinalizeAsync(order, shoppingCartId, paymentProviderName));

        await _shoppingCartHelpers.UpdateAsync(shoppingCartId, cart =>
        {
            cart.Items?.Clear();
            return Task.CompletedTask;
        });

        if (!string.IsNullOrEmpty(paymentProviderName))
        {
            await _paymentProviders
                .Where(provider => provider.Name.EqualsOrdinalIgnoreCase(paymentProviderName))
                .AwaitEachAsync(provider => provider.FinalModificationOfOrderAsync(order, shoppingCartId));
        }
    }

    public async Task<ContentItem?> CreateNoPaymentOrderFromShoppingCartAsync(string shoppingCartId)
    {
        var cart = await _shoppingCartHelpers.RetrieveAsync(shoppingCartId);
        var order = await _contentManager.NewAsync("Order");

        if (await UpdateOrderWithDriversAsync(order)) return null;

        var lineItems = await _shoppingCartHelpers.CreateOrderLineItemsAsync(cart);

        var cartViewModel = await _shoppingCartHelpers.CreateShoppingCartViewModelAsync(
            shoppingCartId,
            order.As<OrderPart>().ShippingAddress.Address,
            order.As<OrderPart>().BillingAddress.Address);

        if (!cartViewModel.Totals.Any() || cartViewModel.Totals.Any(total => total.Value > 0))
        {
            return null;
        }

        await order.AlterAsync<OrderPart>(async orderPart =>
        {
            orderPart.LineItems.SetItems(lineItems);
            orderPart.Status.Text = OrderStatuses.Pending.HtmlClassify();

            await _orderEvents.AwaitEachAsync(orderEvents =>
                orderEvents.CreatedFreeAsnyc(orderPart, cart, cartViewModel));
        });

        await _contentManager.CreateAsync(order);

        return order;
    }

    private async Task<bool> UpdateOrderWithDriversAsync(ContentItem order)
    {
        await _contentItemDisplayManager.UpdateEditorAsync(order, _updateModelAccessor.ModelUpdater, isNew: false);
        return _updateModelAccessor.ModelUpdater.GetModelErrorMessages().Any();
    }

    public async Task UpdateOrderToOrderedAsync(
        ContentItem order,
        string shoppingCartId,
        Func<OrderPart, IEnumerable<IPayment>>? getCharges = null)
    {
        ArgumentNullException.ThrowIfNull(order);

        order.Alter<OrderPart>(orderPart =>
        {
            if (getCharges?.Invoke(orderPart)?.AsList() is { } newCharges)
            {
                orderPart.Charges.SetItems(newCharges);
            }

            orderPart.Status = new TextField { ContentItem = order, Text = OrderStatuses.Ordered.HtmlClassify() };
        });

        await _orderEvents.AwaitEachAsync(orderEvent => orderEvent.OrderedAsync(order, shoppingCartId));
        await _contentManager.UpdateAsync(order);
    }
}
