using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using Stripe;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public class StripeSubscriptionService : IStripeSubscriptionService
{
    private readonly SubscriptionService _subscriptionService = new();
    private readonly IRequestOptionsService _requestOptionsService;
    private readonly IHttpContextAccessor _hca;

    public StripeSubscriptionService(IRequestOptionsService requestOptionsService, IHttpContextAccessor httpContextAccessor)
    {
        _requestOptionsService = requestOptionsService;
        _hca = httpContextAccessor;
    }

    public async Task UpdateSubscriptionAsync(string subscriptionId, SubscriptionUpdateOptions options) =>
        await _subscriptionService.UpdateAsync(
            subscriptionId,
            options,
            requestOptions: await _requestOptionsService.SetIdempotencyKeyAsync(),
            cancellationToken: _hca.HttpContext.RequestAborted);

    public async Task<Subscription> GetSubscriptionAsync(string subscriptionId, SubscriptionGetOptions options) =>
        await _subscriptionService.GetAsync(
            subscriptionId,
            options: options,
            requestOptions: await _requestOptionsService.SetIdempotencyKeyAsync(),
            cancellationToken: _hca.HttpContext.RequestAborted);
}
