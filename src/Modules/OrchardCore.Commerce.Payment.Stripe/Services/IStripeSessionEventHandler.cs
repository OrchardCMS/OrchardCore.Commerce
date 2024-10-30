using Stripe.Checkout;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public interface IStripeSessionEventHandler
{
    Task StripeSessionCreatingAsync(SessionCreateOptions options) => Task.CompletedTask;
    Task StripeSessionCreatedAsync(Session session, SessionCreateOptions options) => Task.CompletedTask;
}
