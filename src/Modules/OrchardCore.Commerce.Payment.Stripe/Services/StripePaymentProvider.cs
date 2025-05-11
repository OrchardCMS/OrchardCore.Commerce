using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Models;
using OrchardCore.Commerce.Payment.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Settings;
using Stripe;
using System;
using System.Threading.Tasks;
using ISession = YesSql.ISession;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public class StripePaymentProvider : IPaymentProvider
{
    public const string ProviderName = "stripe";

    private readonly IPaymentIntentPersistence _paymentIntentPersistence;
    private readonly ISession _session;
    private readonly ISiteService _siteService;
    private readonly IStripePaymentIntentService _stripePaymentIntentService;
    private readonly IStripePaymentService _stripePaymentService;

    public string Name => ProviderName;

    public StripePaymentProvider(
        IPaymentIntentPersistence paymentIntentPersistence,
        ISession session,
        ISiteService siteService,
        IStripePaymentService stripePaymentService,
        IStripePaymentIntentService stripePaymentIntentService)
    {
        _paymentIntentPersistence = paymentIntentPersistence;
        _session = session;
        _siteService = siteService;
        _stripePaymentService = stripePaymentService;
        _stripePaymentIntentService = stripePaymentIntentService;
    }

    public async Task<object> CreatePaymentProviderDataAsync(IPaymentViewModel model, bool isPaymentRequest = false, string shoppingCartId = null)
    {
        PaymentIntent paymentIntent;

        try
        {
            paymentIntent = await _stripePaymentIntentService.CreatePaymentIntentAsync(model.SingleCurrencyTotal, shoppingCartId);
        }
        catch (StripeException exception) when (exception.Message.StartsWithOrdinal("No API key provided."))
        {
            return null;
        }

        if (isPaymentRequest)
        {
            await _session.SaveAsync(new OrderPayment
            {
                OrderId = model.OrderPart.ContentItem.ContentItemId,
                PaymentIntentId = paymentIntent.Id,
            });
        }

        return new StripePaymentProviderData
        {
            PublishableKey = (await _siteService.GetSiteSettingsAsync()).As<StripeApiSettings>().PublishableKey,
            ClientSecret = paymentIntent.ClientSecret,
            PaymentIntentId = paymentIntent.Id,
        };
    }

    public Task ValidateAsync(IUpdateModelAccessor updateModelAccessor, string shoppingCartId, string paymentId = null) =>
        _stripePaymentService.CreateOrUpdateOrderFromShoppingCartAsync(updateModelAccessor, shoppingCartId, paymentId);

    public Task FinalModificationOfOrderAsync(ContentItem order, string shoppingCartId) =>
        // A new payment intent should be created on the next checkout.
        _paymentIntentPersistence.RemoveAsync(shoppingCartId);

    public Task<PaymentOperationStatusViewModel> UpdateAndRedirectToFinishedOrderAsync(
        ContentItem order,
        string shoppingCartId) =>
        throw new NotSupportedException(
            "This code should never be reached, because Stripe payment uses ~/stripe/middleware, not ~/checkout/callback/Stripe.");
}
