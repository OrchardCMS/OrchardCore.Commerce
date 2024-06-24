using Lombiq.HelpfulLibraries.AspNetCore.Exceptions;
using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.Constants;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.MoneyDataType.Extensions;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.ViewModels;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.Tax.Extensions;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Abstractions.Constants.ContentTypes;

namespace OrchardCore.Commerce.Payment.Services;

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
    private readonly IMoneyService _moneyService;
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
        INotifier notifier,
        IMoneyService moneyService)
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
        _moneyService = moneyService;
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

        var email = _hca.HttpContext?.User is { Identity.IsAuthenticated: true } user &&
            await _userManager.GetUserAsync(user) is { } userPrincipal
            ? await _userManager.GetEmailAsync(userPrincipal)
            : string.Empty;

        orderPart.Email.Text = email;
        orderPart.ShippingAddress.UserAddressToSave = nameof(orderPart.ShippingAddress);
        orderPart.BillingAddress.UserAddressToSave = nameof(orderPart.BillingAddress);

        updateOrderPart?.Invoke(orderPart);

        var cart = await _shoppingCartHelpers.CreateShoppingCartViewModelAsync(shoppingCartId, orderPart);
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

        var viewModel = new CheckoutViewModel(orderPart, total, netTotal)
        {
            ShoppingCartId = shoppingCartId,
            Regions = (await _regionService.GetAvailableRegionsAsync()).CreateSelectListOptions(),
            GrossTotal = grossTotal,
            UserEmail = email,
            CheckoutShapes = checkoutShapes,
        };

        if (viewModel.SingleCurrencyTotal.Value > 0)
        {
            await viewModel.WithProviderDataAsync(_paymentProvidersLazy.Value);

            if (!viewModel.PaymentProviderData.Any())
            {
                await _notifier.WarningAsync(new HtmlString(" ").Join(
                    H["There are no applicable payment providers for this site."],
                    H["Please make sure there is at least one enabled and properly configured."]));
            }
        }

        await _checkoutEvents.AwaitEachAsync(checkoutEvents => checkoutEvents.ViewModelCreatedAsync(lines, viewModel));

        return viewModel;
    }

    public async Task<Amount> GetTotalAsync(string? shoppingCartId)
    {
        var (shippingViewModel, billingViewModel) =
            await _updateModelAccessor.ModelUpdater.CreateOrderPartAddressViewModelsAsync();

        var checkoutViewModel = await CreateCheckoutViewModelAsync(
            shoppingCartId,
            part =>
            {
                part.ShippingAddress.Address = shippingViewModel.Address ?? part.ShippingAddress.Address;
                part.BillingAddress.Address = billingViewModel.Address ?? part.BillingAddress.Address;
            });

        return checkoutViewModel?.SingleCurrencyTotal ?? new Amount(0, _moneyService.DefaultCurrency);
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
                .WhereName(paymentProviderName)
                .AwaitEachAsync(provider => provider.FinalModificationOfOrderAsync(order, shoppingCartId));
        }
    }

    public async Task<ContentItem?> CreatePendingOrderFromShoppingCartAsync(
        string? shoppingCartId,
        bool mustBeFree = false,
        bool notifyOnError = true,
        bool throwOnError = false)
    {
        var cart = await _shoppingCartHelpers.RetrieveAsync(shoppingCartId);
        var order = await _contentManager.NewAsync(Order);

        if (!await HandleErrorsAsync(await UpdateOrderWithDriversAsync(order), notifyOnError, throwOnError))
        {
            return null;
        }

        var lineItems = await _shoppingCartHelpers.CreateOrderLineItemsAsync(cart);
        var cartViewModel = await _shoppingCartHelpers.CreateShoppingCartViewModelAsync(shoppingCartId, order);

        if (mustBeFree && cartViewModel.Totals.Any(total => total.Value > 0))
        {
            await _notifier.ErrorAsync(H["Invalid attempt to check out non-free order as free."]);
            return null;
        }

        await order.AlterAsync<OrderPart>(async orderPart =>
        {
            orderPart.LineItems.SetItems(lineItems);
            orderPart.Status.Text = OrderStatusCodes.Pending;

            if (orderPart.BillingAndShippingAddressesMatch.Value)
            {
                // When billing and shipping addresses are set to match and an address field is null, fill out its data
                // with the other field's data. This is to properly store it in the order and display it on the order
                // confirmation page.
                if (orderPart.BillingAddress.Address.Name is null)
                {
                    orderPart.BillingAddress = orderPart.ShippingAddress;
                }

                orderPart.ShippingAddress = orderPart.BillingAddress;
            }

            await _orderEvents.AwaitEachAsync(orderEvents =>
                orderEvents.CreatedFreeAsync(orderPart, cart, cartViewModel));
        });

        await _contentManager.CreateAsync(order);

        return order;
    }

    private async Task<bool> HandleErrorsAsync(IList<string> errors, bool notifyOnError, bool throwOnError)
    {
        if (!errors.Any()) return true;

        if (notifyOnError)
        {
            foreach (var error in errors)
            {
                await _notifier.ErrorAsync(new LocalizedHtmlString(error, error));
            }
        }

        if (throwOnError)
        {
            FrontendException.ThrowIfAny(errors);
        }

        return false;
    }

    public async Task<IList<string>> UpdateOrderWithDriversAsync(ContentItem order)
    {
        await _contentItemDisplayManager.UpdateEditorAsync(order, _updateModelAccessor.ModelUpdater, isNew: false);
        return _updateModelAccessor.ModelUpdater.GetModelErrorMessages()?.AsList() ?? Array.Empty<string>();
    }

    public async Task UpdateOrderToOrderedAsync(
        ContentItem order,
        string? shoppingCartId,
        Func<OrderPart, IEnumerable<Commerce.Abstractions.Models.Payment>?>? getCharges = null)
    {
        ArgumentNullException.ThrowIfNull(order);

        order.Alter<OrderPart>(orderPart =>
        {
            if (getCharges?.Invoke(orderPart)?.AsList() is { } newCharges)
            {
                orderPart.Charges.SetItems(newCharges);
            }

            orderPart.Status = new TextField { ContentItem = order, Text = OrderStatusCodes.Ordered };
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
        var order = await _contentManager.GetAsync(orderId) ?? await _contentManager.NewAsync(Order);
        var isNew = order.IsNew();
        var part = order.As<OrderPart>();

        var cart = await _shoppingCartHelpers.RetrieveAsync(shoppingCartId);
        if (cart.Items.Any() && !order.As<OrderPart>().LineItems.Any())
        {
            await _contentItemDisplayManager.UpdateEditorAsync(order, updateModelAccessor.ModelUpdater, isNew: false);

            var errors = updateModelAccessor.ModelUpdater.GetModelErrorMessages().AsList();
            FrontendException.ThrowIfAny(errors);
        }

        // If there are line items in the Order, use data from Order instead of shopping cart.
        var lineItems = part.LineItems.Any()
            ? part.LineItems
            : await _shoppingCartHelpers.CreateOrderLineItemsAsync(cart);

        var cartViewModel = await _shoppingCartHelpers.CreateShoppingCartViewModelAsync(shoppingCartId, part);

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
