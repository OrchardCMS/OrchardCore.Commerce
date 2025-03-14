using OrchardCore.Commerce.Payment.Stripe.Models;
using OrchardCore.Commerce.Payment.Stripe.ViewModels;
using Stripe;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Abstractions;

/// <summary>
/// Service for managing Stripe subscriptions.
/// </summary>
public interface IStripeSubscriptionService
{
    /// <summary>
    /// Updates a Stripe subscription.
    /// </summary>
    Task UpdateSubscriptionAsync(string subscriptionId, SubscriptionUpdateOptions options);

    /// <summary>
    /// Gets a Stripe subscription.
    /// </summary>
    Task<Subscription> GetSubscriptionAsync(string subscriptionId, SubscriptionGetOptions options);

    /// <summary>
    /// Creates a Stripe subscription using the given <paramref name="options"/>.
    /// </summary>
    /// <returns>The created Stripe <see cref="Subscription"/>.</returns>
    Task<Subscription> CreateSubscriptionAsync(SubscriptionCreateOptions options);

    /// <summary>
    /// Creates a Stripe subscription using the given <paramref name="viewModel"/>.
    /// </summary>
    /// <returns>The created Stripe <see cref="Subscription"/>.</returns>
    Task<SubscriptionCreateResponse> CreateSubscriptionAsync(StripeCreateSubscriptionViewModel viewModel);
}
