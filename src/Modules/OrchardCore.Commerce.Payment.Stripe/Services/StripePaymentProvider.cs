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
    public const string ProviderName = "Stripe";

    private readonly IPaymentIntentPersistence _paymentIntentPersistence;
    private readonly ISession _session;
    private readonly ISiteService _siteService;
    private readonly IStripePaymentService _stripePaymentService;

    public string Name => ProviderName;

    public StripePaymentProvider(
        IPaymentIntentPersistence paymentIntentPersistence,
        ISession session,
        ISiteService siteService,
        IStripePaymentService stripePaymentService)
    {
        _paymentIntentPersistence = paymentIntentPersistence;
        _session = session;
        _siteService = siteService;
        _stripePaymentService = stripePaymentService;
    }

    public async Task<object> CreatePaymentProviderDataAsync(IPaymentViewModel model, bool isPaymentRequest = false)
    {
        PaymentIntent paymentIntent;

        try
        {
            paymentIntent = await _stripePaymentService.CreatePaymentIntentAsync(model.SingleCurrencyTotal);
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

    public Task ValidateAsync(IUpdateModelAccessor updateModelAccessor, string shoppingCartId, string paymentIntentId = null) =>
        _stripePaymentService.CreateOrUpdateOrderFromShoppingCartAsync(updateModelAccessor, shoppingCartId, paymentIntentId);

    public Task FinalModificationOfOrderAsync(ContentItem order, string shoppingCartId)
    {
        // A new payment intent should be created on the next checkout.
        _paymentIntentPersistence.Remove();

        return Task.CompletedTask;
    }

    public Task<PaymentOperationStatusViewModel> UpdateAndRedirectToFinishedOrderAsync(
        ContentItem order,
        string shoppingCartId) =>
        throw new NotSupportedException(
            "This code should never be reached, because Stripe payment uses ~/checkout/middleware/Stripe, not ~/checkout/callback/Stripe.");
}
