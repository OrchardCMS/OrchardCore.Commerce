using OrchardCore.Commerce.Payment.Stripe.Models;
using OrchardCore.Commerce.Payment.Stripe.ViewModels;
using Stripe;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public interface IStripeSubscriptionService
{
    Task UpdateSubscriptionAsync(string subscriptionId, SubscriptionUpdateOptions options);
    Task<Subscription> GetSubscriptionAsync(string subscriptionId, SubscriptionGetOptions options);
    Task<Subscription> CreateSubscriptionAsync(SubscriptionCreateOptions options);
    Task<SubscriptionCreateResponse> CreateSubscriptionAsync(StripeCreateSubscriptionViewModel viewModel);
}
