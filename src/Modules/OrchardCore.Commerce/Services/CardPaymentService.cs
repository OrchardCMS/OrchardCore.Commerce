using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Settings;
using Stripe;
using System;
using System.Collections.Generic;
using System.Globalization;
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

    public CardPaymentService(
        IShoppingCartPersistence shoppingCartPersistence,
        IPriceService priceService,
        IPriceSelectionStrategy priceSelectionStrategy,
        IContentManager contentManager,
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<CardPaymentService> logger)
    {
        _paymentIntentService = new PaymentIntentService();
        _shoppingCartPersistence = shoppingCartPersistence;
        _priceService = priceService;
        _priceSelectionStrategy = priceSelectionStrategy;
        _contentManager = contentManager;
        _siteService = siteService;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
    }

    public async Task<CardPaymentReceiptViewModel> CreatePaymentAndOrderAsync(CardPaymentViewModel viewModel)
    {
        var currentShoppingCart = await _shoppingCartPersistence.RetrieveAsync();
        var totals = await currentShoppingCart.CalculateTotalsAsync(_priceService, _priceSelectionStrategy);

        // Same here as on the checkout page: Later we have to figure out what to do if there are multiple
        // totals i.e., multiple currencies.
        var defaultTotal = totals.FirstOrDefault();

        PaymentIntent finalPaymentIntent;

        try
        {
            var paymentIntentOptions = new PaymentIntentCreateOptions
            {
                // We need to remove the decimal points and convert the value (decimal) to long.
                // https://stripe.com/docs/currencies#zero-decimal
                Amount = long
                    .Parse(
                        string.Join(
                            string.Empty,
                            defaultTotal.ToString().Where(char.IsDigit)),
                        CultureInfo.InvariantCulture),
                Currency = defaultTotal.Currency.CurrencyIsoCode,
                Description = "Orchard Commerce Test Stripe Card Payment",
                ConfirmationMethod = "automatic",
                PaymentMethod = viewModel.PaymentMethod,

                // If shipping is implemented, it needs to be added here too.
                // Shipping =
                ReceiptEmail = viewModel.Email,
            };

            var requestOptions = new RequestOptions
            {
                ApiKey = (await _siteService.GetSiteSettingsAsync())
                    .As<StripeApiSettings>()
                    .SecretKey
                    .DecryptStripeApiKey(_dataProtectionProvider, _logger),
            };

            var paymentIntent = _paymentIntentService.Create(paymentIntentOptions, requestOptions);

            // HERE HANDLE 3DS IF THE STATUS IS requires_action.

            finalPaymentIntent = _paymentIntentService.Confirm(paymentIntent.Id, new PaymentIntentConfirmOptions(), requestOptions);
        }
        catch (StripeException excpetion)
        {
            return ToPaymentReceipt(paymentIntent: null, 0, excpetion);
        }

        var order = await _contentManager.NewAsync("Order");
        var orderId = Guid.NewGuid();

        order.DisplayText = "Order " + orderId;

        // To-do when other parts of the checkout is implemented (notes).
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
                    ChargeText = finalPaymentIntent.Description,
                    TransactionId = finalPaymentIntent.Id,
                    Amount = defaultTotal,
                    Created = finalPaymentIntent.Created,
                });

            //// Shopping cart
            orderPart.LineItems.AddRange(lineItems);

            var orderPartContent = orderPart.Content;

            orderPartContent.OrderId.Text = orderId;

            // To-do when shipping is implemented
            // oderPartContent.BillingAddress
            // oderPartContent.ShippingAddress
        });

        await _contentManager.CreateAsync(order);

        currentShoppingCart.Items.Clear();

        // Shopping cart ID is null by default currently.
        await _shoppingCartPersistence.StoreAsync(currentShoppingCart);

        // Passing decimal value, so we don't need to convert the long back to decimal.
        return ToPaymentReceipt(finalPaymentIntent, defaultTotal.Value);
    }

    private static CardPaymentReceiptViewModel ToPaymentReceipt(
        PaymentIntent paymentIntent,
        decimal value,
        StripeException excpetion = null) =>
        paymentIntent != null
        ? new CardPaymentReceiptViewModel
        {
            Amount = value,
            Currency = paymentIntent.Currency,
            Description = paymentIntent.Description,
            Status = paymentIntent.Status,
            Created = paymentIntent.Created,
            Id = paymentIntent.Id,
            Exception = excpetion,
        }
        : new CardPaymentReceiptViewModel
        {
            Exception = excpetion,
        };
}
