using Stripe.Checkout;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Abstractions;

/// <summary>
/// Service for managing Stripe sessions.
/// </summary>
public interface IStripeSessionService
{
    /// <summary>
    /// Creates a Stripe session using the given <paramref name="options"/>.
    /// </summary>
    /// <returns>The created Stripe <see cref="Session"/>.</returns>
    Task<Session> CreateSessionAsync(SessionCreateOptions options);
}
