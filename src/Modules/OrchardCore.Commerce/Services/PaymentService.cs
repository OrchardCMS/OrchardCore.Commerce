using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Constants;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Settings;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Commerce.Services;

public class PaymentService : IPaymentService
{
    private readonly IShoppingCartHelpers _shoppingCartHelpers;
    private readonly IShoppingCartPersistence _shoppingCartPersistence;
    private readonly IPriceService _priceService;
    private readonly PaymentIntentService _paymentIntentService;
    private readonly IPriceSelectionStrategy _priceSelectionStrategy;
    private readonly IContentManager _contentManager;
    private readonly IStringLocalizer T;
    private readonly ISession _session;
    private readonly IPaymentIntentPersistence _paymentIntentPersistence;
    private readonly RequestOptions _requestOptions;
    private readonly string _siteName;

    // We need to use that many this cannot be avoided.
#pragma warning disable S107 // Methods should not have too many parameters
    public PaymentService(
        IShoppingCartHelpers shoppingCartHelpers,
        IShoppingCartPersistence shoppingCartPersistence,
        IPriceService priceService,
        IPriceSelectionStrategy priceSelectionStrategy,
        IContentManager contentManager,
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<PaymentService> logger,
        IStringLocalizer<PaymentService> stringLocalizer,
        ISession session,
        IPaymentIntentPersistence paymentIntentPersistence)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        _paymentIntentService = new PaymentIntentService();
        _shoppingCartHelpers = shoppingCartHelpers;
        _shoppingCartPersistence = shoppingCartPersistence;
        _priceService = priceService;
        _priceSelectionStrategy = priceSelectionStrategy;
        _contentManager = contentManager;
        _session = session;
        _paymentIntentPersistence = paymentIntentPersistence;
        T = stringLocalizer;

        var siteSettings = siteService.GetSiteSettingsAsync()
            .GetAwaiter()
            .GetResult();
        _siteName = siteSettings.SiteName;
        _requestOptions =
            new RequestOptions
            {
                ApiKey = siteSettings
                    .As<StripeApiSettings>()
                    .SecretKey
                    .DecryptStripeApiKey(dataProtectionProvider, logger),
            };
    }

    public async Task<PaymentIntent> InitializePaymentIntentAsync(string paymentIntentId)
    {
        var totals = (await _shoppingCartHelpers.CreateShoppingCartViewModelAsync(shoppingCartId: null)).Totals;
        CheckTotals(totals);

        // Same here as on the checkout page: Later we have to figure out what to do if there are multiple
        // totals i.e., multiple currencies. https://github.com/OrchardCMS/OrchardCore.Commerce/issues/132
        var defaultTotal = totals.SingleOrDefault();

        var defaultTotalValue = defaultTotal.Value;
        long amountForPayment;
        var currencyType = defaultTotal.Currency.CurrencyIsoCode;

        // If I convert it to conditional expression, it will warn me to extract it again.
#pragma warning disable IDE0045 // Convert to conditional expression
        // We need to convert the value (decimal) to long.
        // https://stripe.com/docs/currencies#zero-decimal
        if (CurrencyCollectionConstants.ZeroDecimalCurrencies.Contains(currencyType))
        {
            amountForPayment = (long)Math.Round(defaultTotalValue);
        }
        else if (CurrencyCollectionConstants.SpecialCases.Contains(currencyType))
        {
            amountForPayment = (long)Math.Round(defaultTotalValue / 100m) * 10000;
        }
        else
        {
            amountForPayment = (long)Math.Round(defaultTotalValue * 100);
        }
#pragma warning restore IDE0045 // Convert to conditional expression

        PaymentIntent paymentIntent;
        if (string.IsNullOrEmpty(paymentIntentId))
        {
            var paymentIntentOptions = new PaymentIntentCreateOptions
            {
                Amount = amountForPayment,
                Currency = defaultTotal.Currency.CurrencyIsoCode,
                Description = T["User started checkout on {0}", _siteName].Value,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true, },
            };

            paymentIntent = await _paymentIntentService.CreateAsync(paymentIntentOptions, _requestOptions);
            _paymentIntentPersistence.Store(paymentIntent.Id);
        }
        else
        {
            var updateOptions = new PaymentIntentUpdateOptions
            {
                Amount = amountForPayment,
                Currency = defaultTotal.Currency.CurrencyIsoCode,
                Description = T["User updated checkout on {0}", _siteName].Value,
            };
            updateOptions.AddExpand("payment_method");
            paymentIntent = await _paymentIntentService.UpdateAsync(paymentIntentId, updateOptions);
        }

        return paymentIntent;
    }

    public Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId)
    {
        var paymentIntentGetOptions = new PaymentIntentGetOptions();
        paymentIntentGetOptions.AddExpand("payment_method");
        return _paymentIntentService.GetAsync(paymentIntentId, paymentIntentGetOptions, _requestOptions);
    }

    public async Task<ContentItem> CreateOrderFromShoppingCartAsync(PaymentIntent paymentIntent)
    {
        var currentShoppingCart = await _shoppingCartPersistence.RetrieveAsync();

        var totals = (await _shoppingCartHelpers.CreateShoppingCartViewModelAsync(shoppingCartId: null)).Totals;
        CheckTotals(totals);

        var defaultTotal = totals.SingleOrDefault();

        var order = await _contentManager.NewAsync("Order");
        var orderId = Guid.NewGuid();

        order.DisplayText = T["Order {0}", orderId];

        IList<OrderLineItem> lineItems = new List<OrderLineItem>();

        // This needs to be done separately because it's async: "Avoid using async lambda when delegate type returns
        // void."
        foreach (var item in currentShoppingCart.Items)
        {
            var trimmedSku = item.ProductSku.Split('-').First();

            var contentItemId = (await _session
                    .QueryIndex<ProductPartIndex>(productPartIndex => productPartIndex.Sku == trimmedSku)
                    .ListAsync())
                .Select(index => index.ContentItemId)
                .First();

            var contentItemVersion = (await _contentManager.GetAsync(contentItemId)).ContentItemVersionId;

            lineItems.Add(await item.CreateOrderLineFromShoppingCartItemAsync(
                _priceSelectionStrategy,
                _priceService,
                contentItemVersion));
        }

        order.Alter<OrderPart>(orderPart =>
        {
            orderPart.Charges.Add(
                new Payment
                {
                    Kind = paymentIntent.PaymentMethod.GetFormattedPaymentType(),
                    ChargeText = paymentIntent.Description,
                    TransactionId = paymentIntent.Id,
                    Amount = defaultTotal,
                    CreatedUtc = paymentIntent.Created,
                });

            // Shopping cart
            orderPart.LineItems.AddRange(lineItems);

            orderPart.OrderId.Text = orderId.ToString();
            orderPart.Status.Text = OrderStatuses.Ordered;
        });

        await _contentManager.CreateAsync(order);

        currentShoppingCart.Items.Clear();

        // Shopping cart ID is null by default currently.
        await _shoppingCartPersistence.StoreAsync(currentShoppingCart);

        // Set back to default.
        _paymentIntentPersistence.Store(paymentIntentId: string.Empty);

        return order;
    }

    private static void CheckTotals(IEnumerable<Amount> totals)
    {
        if (!totals.Any())
        {
            throw new InvalidOperationException("Cannot create a payment without shopping cart total(s)!");
        }
    }
}
