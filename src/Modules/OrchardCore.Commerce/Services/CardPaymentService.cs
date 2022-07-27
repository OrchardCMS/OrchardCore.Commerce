using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Models;
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

    public async Task<PaymentIntent> CreatePaymentAsync(ConfirmPaymentRequest request)
    {
        var currentShoppingCart = await _shoppingCartPersistence.RetrieveAsync();
        var totals = await currentShoppingCart.CalculateTotalsAsync(_priceService, _priceSelectionStrategy);

        // Same here as on the checkout page: Later we have to figure out what to do if there are multiple
        // totals i.e., multiple currencies.
        var defaultTotal = totals.FirstOrDefault();

        var paymentIntent = new PaymentIntent();
        var requestOptions = new RequestOptions();

        if (request.PaymentMethodId != null)
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
                ConfirmationMethod = "manual",
                Confirm = true,
                PaymentMethod = request.PaymentMethodId,

                // If shipping is implemented, it needs to be added here too.
                // Shipping =
                // ReceiptEmail = viewModel.Email,
            };

            paymentIntent = _paymentIntentService.Create(paymentIntentOptions, await CreateRequestOptions());
        }

        if (request.PaymentIntentId != null)
        {
            paymentIntent = _paymentIntentService.Confirm(
                request.PaymentIntentId,
                new PaymentIntentConfirmOptions(),
                await CreateRequestOptions());
        }

        return paymentIntent;
    }

    public async Task CreateOrderFromShoppingCartAsync(PaymentIntent paymentIntent)
    {
        var currentShoppingCart = await _shoppingCartPersistence.RetrieveAsync();
        var totals = await currentShoppingCart.CalculateTotalsAsync(_priceService, _priceSelectionStrategy);
        var defaultTotal = totals.FirstOrDefault();

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
                    ChargeText = paymentIntent.Description,
                    TransactionId = paymentIntent.Id,
                    Amount = defaultTotal,
                    Created = paymentIntent.Created,
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
    }

    private async Task<RequestOptions> CreateRequestOptions() =>
         new RequestOptions
         {
             ApiKey = (await _siteService.GetSiteSettingsAsync())
            .As<StripeApiSettings>()
            .SecretKey
            .DecryptStripeApiKey(_dataProtectionProvider, _logger),
         };

}
