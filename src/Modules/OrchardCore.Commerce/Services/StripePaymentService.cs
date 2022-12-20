using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.Constants;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Fields;
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
using Address = OrchardCore.Commerce.AddressDataType.Address;

namespace OrchardCore.Commerce.Services;

public class StripePaymentService : IStripePaymentService
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
    public StripePaymentService(
        IShoppingCartHelpers shoppingCartHelpers,
        IShoppingCartPersistence shoppingCartPersistence,
        IPriceService priceService,
        IPriceSelectionStrategy priceSelectionStrategy,
        IContentManager contentManager,
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<StripePaymentService> logger,
        IStringLocalizer<StripePaymentService> stringLocalizer,
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

        return string.IsNullOrEmpty(paymentIntentId)
            ? await CreatePaymentIntentAsync(amountForPayment, defaultTotal)
            : await GetOrUpdatePaymentIntentAsync(paymentIntentId, amountForPayment, defaultTotal);
    }

    public Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId)
    {
        var paymentIntentGetOptions = new PaymentIntentGetOptions();
        paymentIntentGetOptions.AddExpansions();
        return _paymentIntentService.GetAsync(
            paymentIntentId,
            paymentIntentGetOptions,
            _requestOptions.SetIdempotencyKey());
    }

    public async Task<ContentItem> UpdateOrderToOrderedAsync(PaymentIntent paymentIntent, Charge charge)
    {
        var orderId = (await _session
                .QueryIndex<OrderPaymentIndex>(index => index.PaymentIntentId == paymentIntent.Id)
                .FirstOrDefaultAsync())
            ?.OrderId;
        var order = await _contentManager.GetAsync(orderId);
        PopulateAddressFieldsIfNeeded(order, charge);
        order.Alter<OrderPart>(orderPart =>
        {
            // Same here as on the checkout page: Later we have to figure out what to do if there are multiple
            // totals i.e., multiple currencies. https://github.com/OrchardCMS/OrchardCore.Commerce/issues/132
            var orderPartCharge = orderPart.Charges.SingleOrDefault();
            var amount = orderPartCharge!.Amount;

            var payment = new Payment
            {
                Kind = paymentIntent.PaymentMethod.GetFormattedPaymentType(),
                ChargeText = paymentIntent.Description,
                TransactionId = paymentIntent.Id,
                Amount = amount,
                CreatedUtc = paymentIntent.Created,
            };

            orderPart.Charges.Remove(orderPartCharge);
            orderPart.Charges.Add(payment);

            orderPart.Status.Text = OrderStatuses.Ordered;
        });

        await _contentManager.UpdateAsync(order);

        return order;
    }

    public async Task<ContentItem> CreateOrUpdateOrderFromShoppingCartAsync(PaymentIntent paymentIntent)
    {
        var currentShoppingCart = await _shoppingCartPersistence.RetrieveAsync();

        var totals = (await _shoppingCartHelpers.CreateShoppingCartViewModelAsync(shoppingCartId: null)).Totals;
        CheckTotals(totals);

        var defaultTotal = totals.SingleOrDefault();

        var orderId = (await _session
                .QueryIndex<OrderPaymentIndex>(index => index.PaymentIntentId == paymentIntent.Id)
                .FirstOrDefaultAsync())
            ?.OrderId;

        ContentItem order;
        string guidId;
        if (string.IsNullOrEmpty(orderId))
        {
            order = await _contentManager.NewAsync("Order");
            guidId = Guid.NewGuid().ToString();

            _session.Save(new OrderPayment { OrderId = order.ContentItemId, PaymentIntentId = paymentIntent.Id });
        }
        else
        {
            order = await _contentManager.GetAsync(orderId);
            guidId = order.As<OrderPart>().OrderId.Text;
        }

        order.DisplayText = T["Order {0}", guidId];

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
                new Payment { Amount = defaultTotal, });

            // Shopping cart
            orderPart.LineItems.AddRange(lineItems);

            orderPart.OrderId.Text = guidId;
            orderPart.Status.Text = OrderStatuses.Pending;
        });

        await _contentManager.CreateAsync(order);

        currentShoppingCart.Items.Clear();

        // Shopping cart ID is null by default currently.
        await _shoppingCartPersistence.StoreAsync(currentShoppingCart);

        // Set back to default, because a new payment intent should be created on the next checkout.
        _paymentIntentPersistence.Store(paymentIntentId: string.Empty);

        return order;
    }

    private static void PopulateAddressFieldsIfNeeded(ContentItem order, Charge charge)
    {
        if (charge == null)
        {
            return;
        }

        order.Alter<OrderPart>(part =>
        {
            part.Email.Text = charge.BillingDetails.Email;
            part.Phone.Text = charge.BillingDetails.Phone;
        });

        var orderPart = order.As<OrderPart>();
        orderPart.Alter<AddressField>("BillingAddress", field =>
        {
            var chargeBilling = charge.BillingDetails;
            field.Address.Name = chargeBilling.Name;
            field.Address.City = chargeBilling.Address.City;
            field.Address.Region = chargeBilling.Address.Country;
            field.Address.StreetAddress1 = chargeBilling.Address.Line1;
            field.Address.StreetAddress2 = chargeBilling.Address.Line2;
            field.Address.Province = chargeBilling.Address.State;
            field.Address.PostalCode = chargeBilling.Address.PostalCode;
            field.UserAddressToSave = nameof(orderPart.BillingAddress);
        });

        orderPart.Alter<AddressField>("ShippingAddress", field =>
        {
            var chargeShipping = charge.Shipping;
            field.Address.Name = chargeShipping.Name;
            field.Address.City = chargeShipping.Address.City;
            field.Address.Region = chargeShipping.Address.Country;
            field.Address.StreetAddress1 = chargeShipping.Address.Line1;
            field.Address.StreetAddress2 = chargeShipping.Address.Line2;
            field.Address.Province = chargeShipping.Address.State;
            field.Address.PostalCode = chargeShipping.Address.PostalCode;
            field.UserAddressToSave = nameof(orderPart.ShippingAddress);
        });
    }

    private async Task<PaymentIntent> CreatePaymentIntentAsync(
        long amountForPayment,
        Amount defaultTotal)
    {
        var paymentIntentOptions = new PaymentIntentCreateOptions
        {
            Amount = amountForPayment,
            Currency = defaultTotal.Currency.CurrencyIsoCode,
            Description = T["User checkout on {0}", _siteName].Value,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true, },
        };

        var paymentIntent = await _paymentIntentService.CreateAsync(
            paymentIntentOptions,
            _requestOptions.SetIdempotencyKey());

        _paymentIntentPersistence.Store(paymentIntent.Id);

        return paymentIntent;
    }

    private async Task<PaymentIntent> GetOrUpdatePaymentIntentAsync(
        string paymentIntentId,
        long amountForPayment,
        Amount defaultTotal)
    {
        var paymentIntent = await GetPaymentIntentAsync(paymentIntentId);

        if (paymentIntent != null &&
            (paymentIntent.Status == PaymentIntentStatuses.Succeeded ||
             paymentIntent.Status == PaymentIntentStatuses.Processing))
        {
            return paymentIntent;
        }

        return await UpdatePaymentIntentAsync(paymentIntentId, amountForPayment, defaultTotal);
    }

    private Task<PaymentIntent> UpdatePaymentIntentAsync(
        string paymentIntentId,
        long amountForPayment,
        Amount defaultTotal)
    {
        var updateOptions = new PaymentIntentUpdateOptions
        {
            Amount = amountForPayment, Currency = defaultTotal.Currency.CurrencyIsoCode,
        };

        updateOptions.AddExpansions();
        return _paymentIntentService.UpdateAsync(
            paymentIntentId,
            updateOptions,
            _requestOptions.SetIdempotencyKey());
    }

    private static void CheckTotals(IEnumerable<Amount> totals)
    {
        if (!totals.Any())
        {
            throw new InvalidOperationException("Cannot create a payment without shopping cart total(s)!");
        }
    }
}
