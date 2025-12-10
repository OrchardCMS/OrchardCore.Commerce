using Stripe;
using System.Threading;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public class DummySubscriptionService : SubscriptionService
{
    public const string TestSubscriptionId = "sub_exampleid000000000000000";
    public static Subscription Subscription { get; set; } = new() { Id = TestSubscriptionId };

    public override Subscription Create(SubscriptionCreateOptions options, RequestOptions requestOptions = null) => new();

    public override Task<Subscription> CreateAsync(
        SubscriptionCreateOptions options,
        RequestOptions requestOptions = null,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Subscription);

    public override Task<Subscription> GetAsync(
        string id,
        SubscriptionGetOptions options = null,
        RequestOptions requestOptions = null,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Subscription);
}
