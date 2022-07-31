using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Money;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Constants;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Settings;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class CardPaymentService : ICardPaymentService
{
    private readonly IShoppingCartPersistence _shoppingCartPersistence;
    private readonly IPriceService _priceService;
    private readonly PaymentIntentService _paymentIntentService;
    private readonly IPriceSelectionStrategy _priceSelectionStrategy;
    private readonly IContentManager _contentManager;
    private readonly ISiteService _siteService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger<CardPaymentService> _logger;
    private readonly IStringLocalizer T;

    // We need to use that many this cannot be avoided.
#pragma warning disable S107 // Methods should not have too many parameters
    public CardPaymentService(
        IShoppingCartPersistence shoppingCartPersistence,
        IPriceService priceService,
        IPriceSelectionStrategy priceSelectionStrategy,
        IContentManager contentManager,
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<CardPaymentService> logger,
        IStringLocalizer<CardPaymentService> stringLocalizer)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        _paymentIntentService = new PaymentIntentService();
        _shoppingCartPersistence = shoppingCartPersistence;
        _priceService = priceService;
        _priceSelectionStrategy = priceSelectionStrategy;
        _contentManager = contentManager;
        _siteService = siteService;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
        T = stringLocalizer;
    }

    public async Task<PaymentIntent> CreatePaymentAsync(ConfirmPaymentRequest request)
    {
        var currentShoppingCart = await _shoppingCartPersistence.RetrieveAsync();
        var totals = await currentShoppingCart.CalculateTotalsAsync(_priceService, _priceSelectionStrategy);

        CheckTotals(totals);

        // Same here as on the checkout page: Later we have to figure out what to do if there are multiple
        // totals i.e., multiple currencies. https://github.com/OrchardCMS/OrchardCore.Commerce/issues/132
        var defaultTotal = totals.SingleOrDefault();

        var siteSettings = await _siteService.GetSiteSettingsAsync();

        var paymentIntent = new PaymentIntent();
        var requestOptions =
            new RequestOptions
            {
                ApiKey = siteSettings
                    .As<StripeApiSettings>()
                    .SecretKey
                    .DecryptStripeApiKey(_dataProtectionProvider, _logger),
            };

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

        if (request.PaymentMethodId != null)
        {
            var paymentIntentOptions = new PaymentIntentCreateOptions
            {
                Amount = amountForPayment,
                Currency = defaultTotal.Currency.CurrencyIsoCode,
                Description = T["Payment for {0}", siteSettings.SiteName].Value,
                ConfirmationMethod = "manual",
                Confirm = true,
                PaymentMethod = request.PaymentMethodId,

                // If shipping is implemented, it needs to be added here too.
                // https://github.com/OrchardCMS/OrchardCore.Commerce/issues/131
                // Shipping =
                // ReceiptEmail = viewModel.Email,
            };

            paymentIntent = await _paymentIntentService.CreateAsync(paymentIntentOptions, requestOptions);
        }

        if (request.PaymentIntentId != null)
        {
            paymentIntent = await _paymentIntentService.ConfirmAsync(
                request.PaymentIntentId,
                new PaymentIntentConfirmOptions(),
                requestOptions);
        }

        return paymentIntent;
    }

    public async Task CreateOrderFromShoppingCartAsync(PaymentIntent paymentIntent)
    {
        var currentShoppingCart = await _shoppingCartPersistence.RetrieveAsync();
        var totals = await currentShoppingCart.CalculateTotalsAsync(_priceService, _priceSelectionStrategy);

        CheckTotals(totals);

        var defaultTotal = totals.SingleOrDefault();

        var order = await _contentManager.NewAsync("Order");
        var orderId = Guid.NewGuid();

        order.DisplayText = "Order " + orderId;

        // To-do when other parts of the checkout is implemented (notes).
        // https://github.com/OrchardCMS/OrchardCore.Commerce/issues/131
        // order.Alter<HtmlBodyPart>(htmlBodyPart => htmlBodyPart.

        IList<OrderLineItem> lineItems = new List<OrderLineItem>();

        // This needs to be done separately because it's async: "Avoid using async lambda when delegate type returns
        // void."
        foreach (var item in currentShoppingCart.Items)
        {
            lineItems.Add(await item.CreateOrderLineFromShoppingCartItemAsync(_priceSelectionStrategy, _priceService));
        }

        order.Alter<OrderPart>(orderPart =>
        {
            orderPart.Charges.Add(
                new CreditCardPayment
                {
                    ChargeText = paymentIntent.Description,
                    TransactionId = paymentIntent.Id,
                    Amount = defaultTotal,
                    CreatedUtc = paymentIntent.Created,
                });

            // Shopping cart
            orderPart.LineItems.AddRange(lineItems);

            var orderPartContent = orderPart.Content;

            orderPartContent.OrderId.Text = orderId;

            // To-do when shipping is implemented. https://github.com/OrchardCMS/OrchardCore.Commerce/issues/131
            // oderPartContent.BillingAddress
            // oderPartContent.ShippingAddress
        });

        await _contentManager.CreateAsync(order);

        currentShoppingCart.Items.Clear();

        // Shopping cart ID is null by default currently.
        await _shoppingCartPersistence.StoreAsync(currentShoppingCart);
    }

    private static void CheckTotals(IEnumerable<Amount> totals)
    {
        if (!totals.Any())
        {
            throw new InvalidOperationException("Cannot create a payment without shopping cart total(s)!");
        }
    }
}
