using OrchardCore.Commerce.Payment.Stripe.Models;
using Stripe;
using Stripe.Checkout;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public interface IStripeSessionEventHandler
{
    Task StripeSessionDataCreatingAsync(StripeSessionData sessionData, Session session, Customer customer);
}
