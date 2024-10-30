using OrchardCore.Commerce.Models;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public interface ISubscriptionService
{
    Task CreateOrUpdateActiveSubscriptionAsync(string subscriptionId, SubscriptionPart subscriptionPart);
}
