using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Constants;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.MoneyDataType.Extensions;
using OrchardCore.Commerce.Payment.Stripe.Services;
using OrchardCore.Commerce.Promotion.Extensions;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Entities;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Settings;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Commerce.Services;

public class StripePaymentService : IStripePaymentService
{
    private readonly IShoppingCartHelpers _shoppingCartHelpers;
    private readonly PaymentIntentService _paymentIntentService;
    private readonly IContentManager _contentManager;
    private readonly ISiteService _siteService;
    private readonly IRequestOptionsService _requestOptionsService;
    private readonly IStringLocalizer T;
    private readonly ISession _session;
    private readonly IPaymentIntentPersistence _paymentIntentPersistence;
    private readonly IContentItemDisplayManager _contentItemDisplayManager;
    private readonly IPaymentService _paymentService;
    private readonly IMoneyService _moneyService;

    // We need to use that many, this cannot be avoided.
#pragma warning disable S107 // Methods should not have too many parameters
    public StripePaymentService(
        IShoppingCartHelpers shoppingCartHelpers,
        IContentManager contentManager,
        ISiteService siteService,
        IRequestOptionsService requestOptionsService,
        IStringLocalizer<StripePaymentService> stringLocalizer,
        ISession session,
        IPaymentIntentPersistence paymentIntentPersistence,
        IContentItemDisplayManager contentItemDisplayManager,
        IPaymentService paymentService,
        IMoneyService moneyService)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        _paymentIntentService = new PaymentIntentService();
        _shoppingCartHelpers = shoppingCartHelpers;
        _contentManager = contentManager;
        _siteService = siteService;
        _requestOptionsService = requestOptionsService;
        _session = session;
        _paymentIntentPersistence = paymentIntentPersistence;
        _moneyService = moneyService;
        T = stringLocalizer;
        _contentItemDisplayManager = contentItemDisplayManager;
        _paymentService = paymentService;
    }

    public async Task<string> CreateClientSecretAsync(Amount total, ShoppingCartViewModel cart)
    {
        var stripeApiSettings = (await _siteService.GetSiteSettingsAsync()).As<StripeApiSettings>();

        if (string.IsNullOrEmpty(stripeApiSettings.PublishableKey) ||
            string.IsNullOrEmpty(stripeApiSettings.SecretKey) ||
            total.Value <= 0)
        {
            return null;
        }

        var paymentIntentId = _paymentIntentPersistence.Retrieve();
        var totals = CheckTotals(cart);

        // Same here as on the checkout page: Later we have to figure out what to do if there are multiple
        // totals i.e., multiple currencies. https://github.com/OrchardCMS/OrchardCore.Commerce/issues/132
        var defaultTotal = totals.SingleOrDefault();

        var initPaymentIntent = string.IsNullOrEmpty(paymentIntentId)
            ? await CreatePaymentIntentAsync(defaultTotal)
            : await GetOrUpdatePaymentIntentAsync(paymentIntentId, defaultTotal);

        return initPaymentIntent.ClientSecret;
    }

    public async Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId)
    {
        var paymentIntentGetOptions = new PaymentIntentGetOptions();
        paymentIntentGetOptions.AddExpansions();
        return await _paymentIntentService.GetAsync(
            paymentIntentId,
            paymentIntentGetOptions,
            await _requestOptionsService.SetIdempotencyKeyAsync());
    }

    public async Task UpdateOrderToOrderedAsync(PaymentIntent paymentIntent) =>
        await _paymentService.UpdateOrderToOrderedAsync(
            await GetOrderByPaymentIntentIdAsync(paymentIntent.Id),
            shoppingCartId: null,
            CreateChargesProvider(paymentIntent));

    public async Task<IActionResult> UpdateAndRedirectToFinishedOrderAsync(
        Controller controller,
        ContentItem order,
        PaymentIntent paymentIntent) =>
        await _paymentService.UpdateAndRedirectToFinishedOrderAsync(
            controller,
            order,
            shoppingCartId: null,
            StripePaymentProvider.ProviderName,
            CreateChargesProvider(paymentIntent));

    private Func<OrderPart, IEnumerable<IPayment>?> CreateChargesProvider(PaymentIntent paymentIntent) =>
        orderPart =>
        {
            // Same here as on the checkout page: Later we have to figure out what to do if there are multiple
            // totals i.e., multiple currencies. https://github.com/OrchardCMS/OrchardCore.Commerce/issues/132
            var amount = orderPart.Charges.Single().Amount;

            return new[] { paymentIntent.CreatePayment(amount) };
        };

    public async Task UpdateOrderToPaymentFailedAsync(PaymentIntent paymentIntent)
    {
        var order = await GetOrderByPaymentIntentIdAsync(paymentIntent.Id);
        order.Alter<OrderPart>(orderPart =>
            orderPart.Status = new TextField { ContentItem = order, Text = OrderStatuses.PaymentFailed.HtmlClassify() });

        await _contentManager.UpdateAsync(order);
    }

    public Task<OrderPayment> GetOrderPaymentByPaymentIntentIdAsync(string paymentIntentId) =>
        _session
            .Query<OrderPayment, OrderPaymentIndex>(index => index.PaymentIntentId == paymentIntentId)
            .FirstOrDefaultAsync();

    public async Task<ContentItem> CreateOrUpdateOrderFromShoppingCartAsync(IUpdateModelAccessor updateModelAccessor)
    {
        var paymentIntent = await GetPaymentIntentAsync(_paymentIntentPersistence.Retrieve());

        // Stripe doesn't support multiple shopping cart IDs because we can't send that info to the middleware anyway.
        var currentShoppingCart = await _shoppingCartHelpers.RetrieveAsync(shoppingCartId: null);
        var orderId = (await GetOrderPaymentByPaymentIntentIdAsync(paymentIntent.Id))?.OrderId;

        if (!string.IsNullOrEmpty(orderId) && await _contentManager.GetAsync(orderId) is { } order)
        {
            // If there are line items in the Order, use data from Order instead of shopping cart.
            if (!order.As<OrderPart>().LineItems.Any() &&
                currentShoppingCart.Items.Any() &&
                await UpdateOrderWithDriversAsync(order, updateModelAccessor))
            {
                return null;
            }
        }
        else
        {
            order = await _contentManager.NewAsync("Order");
            if (await UpdateOrderWithDriversAsync(order, updateModelAccessor))
            {
                return null;
            }

            _session.Save(new OrderPayment
            {
                OrderId = order.ContentItemId,
                PaymentIntentId = paymentIntent.Id,
            });
        }

        var orderPart = order.As<OrderPart>();

        var lineItems = await _shoppingCartHelpers.CreateOrderLineItemsAsync(currentShoppingCart);
        if (!lineItems.Any())
        {
            lineItems = orderPart.LineItems;
        }

        var cartViewModel = await _shoppingCartHelpers.CreateShoppingCartViewModelAsync(
            shoppingCartId: null,
            orderPart.ShippingAddress.Address,
            orderPart.BillingAddress.Address);

        var currency = orderPart.LineItems.Any() ? orderPart.LineItems[0].LinePrice.Currency : _moneyService.DefaultCurrency;
        var orderTotals = new Amount(0, currency);

        if (cartViewModel is null)
        {
            orderTotals = orderPart.LineItems.Select(item => item.LinePrice).Sum();
        }

        // If there is no cart, use current Order's data.
        var defaultTotal = cartViewModel is null ? orderTotals : CheckTotals(cartViewModel).SingleOrDefault();
        AlterOrder(order, paymentIntent, defaultTotal, cartViewModel, lineItems);

        if (string.IsNullOrEmpty(orderId))
        {
            await _contentManager.CreateAsync(order);
        }
        else
        {
            await _contentManager.UpdateAsync(order);
        }

        return order;
    }

    private static long GetPaymentAmount(Amount total)
    {
        if (CurrencyCollectionConstants.ZeroDecimalCurrencies.Contains(total.Currency.CurrencyIsoCode))
        {
            return (long)Math.Round(total.Value);
        }

        return CurrencyCollectionConstants.SpecialCases.Contains(total.Currency.CurrencyIsoCode)
            ? (long)Math.Round(total.Value / 100m) * 10000
            : (long)Math.Round(total.Value * 100);
    }

    private async Task<bool> UpdateOrderWithDriversAsync(ContentItem order, IUpdateModelAccessor updateModelAccessor)
    {
        await _contentItemDisplayManager.UpdateEditorAsync(order, updateModelAccessor.ModelUpdater, isNew: false);

        return updateModelAccessor.ModelUpdater.GetModelErrorMessages().Any();
    }

    public async Task<PaymentIntent> CreatePaymentIntentAsync(Amount total)
    {
        var siteSettings = await _siteService.GetSiteSettingsAsync();
        var paymentIntentOptions = new PaymentIntentCreateOptions
        {
            Amount = GetPaymentAmount(total),
            Currency = total.Currency.CurrencyIsoCode,
            Description = T["User checkout on {0}", siteSettings.SiteName].Value,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true, },
        };

        var paymentIntent = await _paymentIntentService.CreateAsync(
            paymentIntentOptions,
            await _requestOptionsService.SetIdempotencyKeyAsync());

        _paymentIntentPersistence.Store(paymentIntent.Id);

        return paymentIntent;
    }

    private static void AlterOrder(
        ContentItem order,
        PaymentIntent paymentIntent,
        Amount defaultTotal,
        ShoppingCartViewModel cartViewModel,
        IEnumerable<OrderLineItem> lineItems)
    {
        order.Alter<OrderPart>(orderPart =>
        {
            orderPart.Charges.Clear();
            orderPart.Charges.Add(paymentIntent.CreatePayment(defaultTotal));

            if (cartViewModel is null) return;

            // Shopping cart
            orderPart.LineItems.SetItems(lineItems);
            orderPart.Status = new TextField { ContentItem = order, Text = OrderStatuses.Pending.HtmlClassify() };

            // Store the current applicable discount info so they will be available in the future.
            orderPart.AdditionalData.SetDiscountsByProduct(cartViewModel
                .Lines
                .Where(line => line.AdditionalData.GetDiscounts().Any())
                .ToDictionary(
                    line => line.ProductSku,
                    line => line.AdditionalData.GetDiscounts()));
        });

        order.Alter<StripePaymentPart>(part => part.PaymentIntentId = new TextField { ContentItem = order, Text = paymentIntent.Id });
    }

    private async Task<PaymentIntent> GetOrUpdatePaymentIntentAsync(
        string paymentIntentId,
        Amount defaultTotal)
    {
        var paymentIntent = await GetPaymentIntentAsync(paymentIntentId);

        if (paymentIntent?.Status is PaymentIntentStatuses.Succeeded or PaymentIntentStatuses.Processing)
        {
            return paymentIntent;
        }

        return await UpdatePaymentIntentAsync(paymentIntentId, defaultTotal);
    }

    private async Task<PaymentIntent> UpdatePaymentIntentAsync(
        string paymentIntentId,
        Amount total)
    {
        var updateOptions = new PaymentIntentUpdateOptions
        {
            Amount = GetPaymentAmount(total),
            Currency = total.Currency.CurrencyIsoCode,
        };

        updateOptions.AddExpansions();
        return await _paymentIntentService.UpdateAsync(
            paymentIntentId,
            updateOptions,
            await _requestOptionsService.SetIdempotencyKeyAsync());
    }

    private static IList<Amount> CheckTotals(ShoppingCartViewModel viewModel)
    {
        if (!viewModel.Totals.Any())
        {
            throw new InvalidOperationException("Cannot create a payment without shopping cart total(s)!");
        }

        return viewModel.Totals;
    }

    private async Task<ContentItem> GetOrderByPaymentIntentIdAsync(string paymentIntentId)
    {
        var orderId = (await GetOrderPaymentByPaymentIntentIdAsync(paymentIntentId))?.OrderId;
        return await _contentManager.GetAsync(orderId);
    }
}
