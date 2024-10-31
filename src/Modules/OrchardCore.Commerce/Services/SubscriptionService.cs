using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using System.Threading.Tasks;
using YesSql;
using static OrchardCore.Commerce.Abstractions.Constants.ContentTypes;

namespace OrchardCore.Commerce.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly IContentManager _contentManager;
    private readonly ISession _session;

    public SubscriptionService(IContentManager contentManager, ISession session)
    {
        _contentManager = contentManager;
        _session = session;
    }

    public async Task CreateOrUpdateSubscriptionAsync(string idInPaymentProvider, SubscriptionPart subscriptionPart)
    {
        var subscription = await _session.Query<ContentItem, SubscriptionPartIndex>(
                item => item.IdInPaymentProvider == idInPaymentProvider)
            .FirstOrDefaultAsync();

        subscription ??= await _contentManager.NewAsync(Subscription);
        subscription.Apply(subscriptionPart);
        if (subscription.IsNew())
        {
            await _contentManager.CreateAsync(subscription);
        }
        else
        {
            await _contentManager.UpdateAsync(subscription);
        }
    }
}
