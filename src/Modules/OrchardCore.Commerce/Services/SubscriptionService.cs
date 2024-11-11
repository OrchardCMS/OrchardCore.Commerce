using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using System.Collections.Generic;
using System.Threading.Tasks;
using YesSql;
using static OrchardCore.Commerce.Abstractions.Constants.ContentTypes;

namespace OrchardCore.Commerce.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly IContentManager _contentManager;
    private readonly ISession _session;
    private readonly IClock _clock;

    public SubscriptionService(IContentManager contentManager, ISession session, IClock clock)
    {
        _contentManager = contentManager;
        _session = session;
        _clock = clock;
    }

    public async Task CreateOrUpdateSubscriptionAsync(string idInPaymentProvider, SubscriptionPart subscriptionPart)
    {
        var subscription = await _session.Query<ContentItem, SubscriptionPartIndex>(
                item => item.IdInPaymentProvider == idInPaymentProvider)
            .FirstOrDefaultAsync();

        // Get current subscription if exists, if exists do not override start date
        var startDate = subscription.As<SubscriptionPart>().StartDateUtc.Value;
        subscriptionPart.StartDateUtc.Value = startDate ?? _clock.UtcNow;

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
