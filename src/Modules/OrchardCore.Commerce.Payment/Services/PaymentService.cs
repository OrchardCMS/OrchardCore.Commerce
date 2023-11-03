using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Constants;
using OrchardCore.Commerce.Exceptions;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Extensions;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Tax.Extensions;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
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
    private readonly IContentManager _contentManager;
    private readonly IShoppingCartHelpers _shoppingCartHelpers;
    private readonly UserManager<IUser> _userManager;
    private readonly IRegionService _regionService;
    private readonly IHttpContextAccessor _hca;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IContentItemDisplayManager _contentItemDisplayManager;
    private readonly IEnumerable<IOrderEvents> _orderEvents;
    private readonly Lazy<IEnumerable<IPaymentProvider>> _paymentProvidersLazy;
    private readonly IEnumerable<ICheckoutEvents> _checkoutEvents;
    private readonly INotifier _notifier;
    private readonly IHtmlLocalizer H;

    // We need all of them.
#pragma warning disable S107 // Methods should not have too many parameters
    public PaymentService(
        IFieldsOnlyDisplayManager fieldsOnlyDisplayManager,
        IOrchardServices<PaymentService> services,
        IShoppingCartHelpers shoppingCartHelpers,
        IRegionService regionService,
        IUpdateModelAccessor updateModelAccessor,
        IContentItemDisplayManager contentItemDisplayManager,
        IEnumerable<IOrderEvents> orderEvents,
        Lazy<IEnumerable<IPaymentProvider>> paymentProvidersLazy,
        IEnumerable<ICheckoutEvents> checkoutEvents,
        INotifier notifier)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        _fieldsOnlyDisplayManager = fieldsOnlyDisplayManager;
        _contentManager = services.ContentManager.Value;
        _shoppingCartHelpers = shoppingCartHelpers;
        _userManager = services.UserManager.Value;
        _regionService = regionService;
        _updateModelAccessor = updateModelAccessor;
        _contentItemDisplayManager = contentItemDisplayManager;
        _orderEvents = orderEvents;
        _paymentProvidersLazy = paymentProvidersLazy;
        _checkoutEvents = checkoutEvents;
        _notifier = notifier;
        _hca = services.HttpContextAccessor.Value;
        H = services.HtmlLocalizer.Value;
    }

    public async Task<ICheckoutViewModel?> CreateCheckoutViewModelAsync(
        string? shoppingCartId,
        Action<OrderPart>? updateOrderPart = null)
    {
        var orderPart = new OrderPart();

        await _checkoutEvents.AwaitEachAsync(checkoutEvents =>
            checkoutEvents.OrderCreatingAsync(orderPart, shoppingCartId));

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

        var viewModel = new CheckoutViewModel(orderPart, total, netTotal)
        {
            ShoppingCartId = shoppingCartId,
            Regions = (await _regionService.GetAvailableRegionsAsync()).CreateSelectListOptions(),
            GrossTotal = grossTotal,
            UserEmail = email,
            CheckoutShapes = checkoutShapes,
        };

        if (viewModel.SingleCurrencyTotal.Value > 0) await viewModel.WithProviderDataAsync(_paymentProvidersLazy.Value);

        return viewModel;
    }

    public async Task FinalModificationOfOrderAsync(ContentItem order, string? shoppingCartId, string? paymentProviderName)
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
            await _paymentProvidersLazy
                .Value
                .Where(provider => provider.Name.EqualsOrdinalIgnoreCase(paymentProviderName))
                .AwaitEachAsync(provider => provider.FinalModificationOfOrderAsync(order, shoppingCartId));
        }
    }

    public async Task<ContentItem?> CreateNoPaymentOrderFromShoppingCartAsync(string? shoppingCartId)
    {
        var cart = await _shoppingCartHelpers.RetrieveAsync(shoppingCartId);
        var order = await _contentManager.NewAsync("Order");

        var errors = await UpdateOrderWithDriversAsync(order);
        if (errors.Any())
        {
            foreach (var error in errors)
            {
                await _notifier.ErrorAsync(new LocalizedHtmlString(error, error));
            }

            return null;
        }

        var lineItems = await _shoppingCartHelpers.CreateOrderLineItemsAsync(cart);

        var cartViewModel = await _shoppingCartHelpers.CreateShoppingCartViewModelAsync(
            shoppingCartId,
            order.As<OrderPart>().ShippingAddress.Address,
            order.As<OrderPart>().BillingAddress.Address);

        if (!cartViewModel.Totals.Any() || cartViewModel.Totals.Any(total => total.Value > 0))
        {
            await _notifier.ErrorAsync(H["Invalid attempt to check out non-free order as free."]);
            return null;
        }

        await order.AlterAsync<OrderPart>(async orderPart =>
        {
            orderPart.LineItems.SetItems(lineItems);
            orderPart.Status.Text = OrderStatuses.Pending.HtmlClassify();

            await _orderEvents.AwaitEachAsync(orderEvents =>
                orderEvents.CreatedFreeAsync(orderPart, cart, cartViewModel));
        });

        await _contentManager.CreateAsync(order);

        return order;
    }

    private async Task<IList<string>> UpdateOrderWithDriversAsync(ContentItem order)
    {
        await _contentItemDisplayManager.UpdateEditorAsync(order, _updateModelAccessor.ModelUpdater, isNew: false);
        return _updateModelAccessor.ModelUpdater.GetModelErrorMessages()?.AsList() ?? Array.Empty<string>();
    }

    public async Task UpdateOrderToOrderedAsync(
        ContentItem order,
        string? shoppingCartId,
        Func<OrderPart, IEnumerable<IPayment>?>? getCharges = null)
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

    public async Task<(ContentItem Order, bool IsNew)> CreateOrUpdateOrderFromShoppingCartAsync(
        IUpdateModelAccessor updateModelAccessor,
        string? orderId,
        string? shoppingCartId,
        AlterOrderAsyncDelegate? alterOrderAsync = null)
    {
        var order = await _contentManager.GetAsync(orderId) ?? await _contentManager.NewAsync("Order");
        var isNew = order.IsNew();
        var part = order.As<OrderPart>();

        var cart = await _shoppingCartHelpers.RetrieveAsync(shoppingCartId);
        if (cart.Items.Any() && !order.As<OrderPart>().LineItems.Any())
        {
            await _contentItemDisplayManager.UpdateEditorAsync(order, updateModelAccessor.ModelUpdater, isNew: false);

            var errors = updateModelAccessor.ModelUpdater.GetModelErrorMessages().AsList();
            if (errors.Any())
            {
                throw new FrontendException(new HtmlString("<br>").Join(
                    errors.Select(error => H["{0}", error]).ToArray()));
            }
        }

        // If there are line items in the Order, use data from Order instead of shopping cart.
        var lineItems = part.LineItems.Any()
            ? part.LineItems
            : await _shoppingCartHelpers.CreateOrderLineItemsAsync(cart);

        var cartViewModel = await _shoppingCartHelpers.CreateShoppingCartViewModelAsync(
            shoppingCartId,
            part.ShippingAddress.Address,
            part.BillingAddress.Address);

        // If there is no cart, use current Order's data.
        var total = cartViewModel is null
            ? part.LineItems.Select(item => item.LinePrice).Sum()
            : cartViewModel.GetTotalsOrThrowIfEmpty().SingleOrDefault();

        if (alterOrderAsync is not null) await alterOrderAsync(order, isNew, total, cartViewModel, lineItems);

        if (isNew)
        {
            await _contentManager.CreateAsync(order);
        }
        else
        {
            await _contentManager.UpdateAsync(order);
        }

        return (order, isNew);
    }
}
