using OrchardCore.Commerce.Models;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

/// <summary>
/// Subscription content type services.
/// </summary>
public interface ISubscriptionService
{
    /// <summary>
    /// Creates or updates a subscription if <paramref name="idInPaymentProvider"/> exists as a subscription content item
    /// with the provided <see cref="SubscriptionPart"/>.
    /// </summary>
    Task CreateOrUpdateSubscriptionAsync(string idInPaymentProvider, SubscriptionPart subscriptionPart);
}
