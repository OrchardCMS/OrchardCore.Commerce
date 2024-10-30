using Stripe.Checkout;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public interface IStripeSessionService
{
    Task<Session> CreateSessionAsync(SessionCreateOptions options);
}
