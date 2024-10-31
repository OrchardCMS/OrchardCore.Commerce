using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using Stripe.Checkout;
using System.Collections.Generic;
using System.Threading.Tasks;
using Session = Stripe.Checkout.Session;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public class StripeSessionService : IStripeSessionService
{
    private readonly SessionService _sessionService = new();
    private readonly IRequestOptionsService _requestOptionsService;
    private readonly IHttpContextAccessor _hca;
    private readonly IEnumerable<IStripeSessionEventHandler> _stripeSessionEventHandlers;

    public StripeSessionService(
        IRequestOptionsService requestOptionsService,
        IHttpContextAccessor httpContextAccessor,
        IEnumerable<IStripeSessionEventHandler> stripeSessionEventHandlers)
    {
        _requestOptionsService = requestOptionsService;
        _hca = httpContextAccessor;
        _stripeSessionEventHandlers = stripeSessionEventHandlers;
    }

    public async Task<Session> CreateSessionAsync(SessionCreateOptions options)
    {
        await _stripeSessionEventHandlers.AwaitEachAsync(handler => handler.StripeSessionCreatingAsync(options));

        var session = await _sessionService.CreateAsync(
            options,
            await _requestOptionsService.SetIdempotencyKeyAsync(),
            cancellationToken: _hca.HttpContext!.RequestAborted);

        await _stripeSessionEventHandlers.AwaitEachAsync(handler => handler.StripeSessionCreatedAsync(session, options));
        return session;
    }
}
