using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Abstractions.Constants;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Abstractions.ViewModels;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.Constants;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Constants;
using OrchardCore.Commerce.Payment.Stripe.Extensions;
using OrchardCore.Commerce.Payment.Stripe.Indexes;
using OrchardCore.Commerce.Payment.Stripe.Models;
using OrchardCore.Commerce.Promotion.Extensions;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Settings;
using Stripe;
using YesSql;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public class StripePaymentService : IStripePaymentService
{
    private readonly PaymentIntentService _paymentIntentService = new();

    private readonly IContentManager _contentManager;
    private readonly ISiteService _siteService;
    private readonly IRequestOptionsService _requestOptionsService;
    private readonly IStringLocalizer T;
    private readonly ISession _session;
    private readonly IPaymentIntentPersistence _paymentIntentPersistence;
    private readonly IPaymentService _paymentService;

    public StripePaymentService(
        IContentManager contentManager,
        ISiteService siteService,
        IRequestOptionsService requestOptionsService,
        IStringLocalizer<StripePaymentService> stringLocalizer,
        ISession session,
        IPaymentIntentPersistence paymentIntentPersistence,
        IPaymentService paymentService)
    {
        _contentManager = contentManager;
        _siteService = siteService;
        _requestOptionsService = requestOptionsService;
        _session = session;
        _paymentIntentPersistence = paymentIntentPersistence;
        T = stringLocalizer;
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
        var totals = cart.GetTotalsOrThrowIfEmpty();

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

    public Task<IActionResult> UpdateAndRedirectToFinishedOrderAsync(
        Controller controller,
        ContentItem order,
        PaymentIntent paymentIntent) =>
        _paymentService.UpdateAndRedirectToFinishedOrderAsync(
            controller,
            order,
            shoppingCartId: null,
            StripePaymentProvider.ProviderName,
            CreateChargesProvider(paymentIntent));

    private static Func<OrderPart, IEnumerable<Commerce.Abstractions.Models.Payment>> CreateChargesProvider(PaymentIntent paymentIntent) =>
        orderPart => orderPart.Charges.Select(charge => paymentIntent.CreatePayment(charge.Amount));

    public async Task UpdateOrderToPaymentFailedAsync(PaymentIntent paymentIntent)
    {
        var order = await GetOrderByPaymentIntentIdAsync(paymentIntent.Id);
        order.Alter<OrderPart>(orderPart =>
            orderPart.Status = new TextField { ContentItem = order, Text = OrderStatusCodes.PaymentFailed });

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
        var (order, isNew) = await _paymentService.CreateOrUpdateOrderFromShoppingCartAsync(
            updateModelAccessor,
            (await GetOrderPaymentByPaymentIntentIdAsync(paymentIntent.Id))?.OrderId,
            shoppingCartId: null,
            (order, _, total, cartViewModel, lineItems) =>
            {
                order.Alter<OrderPart>(orderPart =>
                {
                    orderPart.Charges.Clear();
                    orderPart.Charges.Add(paymentIntent.CreatePayment(total));

                    if (cartViewModel is null) return;

                    // Shopping cart
                    orderPart.LineItems.SetItems(lineItems);
                    orderPart.Status = new TextField { ContentItem = order, Text = OrderStatusCodes.Pending };

                    // Store the current applicable discount info so they will be available in the future.
                    orderPart.AdditionalData.SetDiscountsByProduct(cartViewModel
                        .Lines
                        .Where(line => line.AdditionalData.GetDiscounts().Any())
                        .ToDictionary(
                            line => line.ProductSku,
                            line => line.AdditionalData.GetDiscounts()));
                });

                order.Alter<StripePaymentPart>(part =>
                    part.PaymentIntentId = new TextField { ContentItem = order, Text = paymentIntent.Id });

                return Task.CompletedTask;
            });

        if (!order.As<OrderPart>().LineItems.Any())
        {
            updateModelAccessor.ModelUpdater.ModelState.AddModelError(
                nameof(OrderPart.LineItems),
                T["The order is empty."].Value);
        }

        if (isNew)
        {
            await _session.SaveAsync(new OrderPayment
            {
                OrderId = order.ContentItemId,
                PaymentIntentId = paymentIntent.Id,
            });
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

    private async Task<PaymentIntent> GetOrUpdatePaymentIntentAsync(
        string paymentIntentId,
        Amount defaultTotal)
    {
        var paymentIntent = await GetPaymentIntentAsync(paymentIntentId);

        if (paymentIntent?.Status is PaymentIntentStatuses.Succeeded or PaymentIntentStatuses.Processing)
        {
            return paymentIntent;
        }

        var updateOptions = new PaymentIntentUpdateOptions
        {
            Amount = GetPaymentAmount(defaultTotal),
            Currency = defaultTotal.Currency.CurrencyIsoCode,
        };

        updateOptions.AddExpansions();
        return await _paymentIntentService.UpdateAsync(
            paymentIntentId,
            updateOptions,
            await _requestOptionsService.SetIdempotencyKeyAsync());
    }

    private async Task<ContentItem> GetOrderByPaymentIntentIdAsync(string paymentIntentId)
    {
        var orderId = (await GetOrderPaymentByPaymentIntentIdAsync(paymentIntentId))?.OrderId;
        return await _contentManager.GetAsync(orderId);
    }
}
