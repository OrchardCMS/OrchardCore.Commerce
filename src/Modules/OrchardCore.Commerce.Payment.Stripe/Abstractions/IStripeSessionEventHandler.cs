using Stripe.Checkout;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Abstractions;

/// <summary>
/// Event handler for Stripe sessions.
/// </summary>
public interface IStripeSessionEventHandler
{
    /// <summary>
    /// Called before a Stripe session is created with a prepopulated <see cref="SessionCreateOptions"/>
    /// <paramref name="options"/>. Here you can modify the options before the session is created.
    /// </summary>
    Task StripeSessionCreatingAsync(SessionCreateOptions options) => Task.CompletedTask;

    /// <summary>
    /// Called after a Stripe session is created with the created <paramref name="session"/> and the
    /// <paramref name="options"/> used during creation.
    /// </summary>
    Task StripeSessionCreatedAsync(Session session, SessionCreateOptions options) => Task.CompletedTask;
}
