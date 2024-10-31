using OrchardCore.Commerce.MoneyDataType;
using Stripe;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Abstractions;

public interface IStripePaymentIntentService
{
    Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId);

    Task<PaymentIntent> CreatePaymentIntentAsync(Amount total);
    Task<PaymentIntent> CreatePaymentIntentAsync(PaymentIntentCreateOptions options);

    Task<PaymentIntent> GetOrUpdatePaymentIntentAsync(
        string paymentIntentId,
        Amount defaultTotal);
}
