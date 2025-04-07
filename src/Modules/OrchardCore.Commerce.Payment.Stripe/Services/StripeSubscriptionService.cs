using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Models;
using OrchardCore.Commerce.Payment.Stripe.ViewModels;
using Stripe;
using System.Threading;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public class StripeSubscriptionService : IStripeSubscriptionService
{
    private readonly SubscriptionService _subscriptionService;
    private readonly IRequestOptionsService _requestOptionsService;
    private readonly IHttpContextAccessor _hca;

    private CancellationToken Aborted => _hca?.HttpContext?.RequestAborted ?? CancellationToken.None;

    public StripeSubscriptionService(
        SubscriptionService subscriptionService,
        IRequestOptionsService requestOptionsService,
        IHttpContextAccessor httpContextAccessor)
    {
        _subscriptionService = subscriptionService;
        _requestOptionsService = requestOptionsService;
        _hca = httpContextAccessor;
    }

    public async Task<SubscriptionCreateResponse> CreateSubscriptionAsync(StripeCreateSubscriptionViewModel viewModel)
    {
        // Automatically save the payment method to the subscription
        // when the first payment is successful.
        var paymentSettings = new SubscriptionPaymentSettingsOptions
        {
            SaveDefaultPaymentMethod = "on_subscription",
        };

        var subscriptionOptions = new SubscriptionCreateOptions
        {
            Customer = viewModel.CustomerId,
            PaymentSettings = paymentSettings,
            PaymentBehavior = "default_incomplete",
        };

        foreach (var priceId in viewModel.PriceIds)
        {
            subscriptionOptions.Items.Add(new SubscriptionItemOptions { Price = priceId });
        }

        subscriptionOptions.AddExpand("latest_invoice.payment_intent");
        subscriptionOptions.AddExpand("pending_setup_intent");

        var subscription = await _subscriptionService.CreateAsync(
            subscriptionOptions,
            requestOptions: await _requestOptionsService.SetIdempotencyKeyAsync(),
            cancellationToken: Aborted);

        if (subscription.PendingSetupIntent != null)
        {
            return new SubscriptionCreateResponse
            {
                Type = "setup",
                ClientSecret = subscription.PendingSetupIntent.ClientSecret,
            };
        }

        return new SubscriptionCreateResponse
        {
            Type = "payment",
            ClientSecret = subscription.LatestInvoice.PaymentIntent.ClientSecret,
        };
    }

    public async Task<Subscription> CreateSubscriptionAsync(SubscriptionCreateOptions options = null) =>
        await _subscriptionService.CreateAsync(
            options,
            requestOptions: await _requestOptionsService.SetIdempotencyKeyAsync(),
            cancellationToken: Aborted);

    public async Task UpdateSubscriptionAsync(string subscriptionId, SubscriptionUpdateOptions options = null) =>
        await _subscriptionService.UpdateAsync(
            subscriptionId,
            options,
            requestOptions: await _requestOptionsService.SetIdempotencyKeyAsync(),
            cancellationToken: Aborted);

    public async Task<Subscription> GetSubscriptionAsync(string subscriptionId, SubscriptionGetOptions options = null) =>
        await _subscriptionService.GetAsync(
            subscriptionId,
            options: options,
            requestOptions: await _requestOptionsService.SetIdempotencyKeyAsync(),
            cancellationToken: Aborted);
}
