using Stripe;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public class StripeHelperService : IStripeHelperService
{
    public Event PrepareStripeEvent(string json, string stripeSignatureHeader, string secret, bool throwOnApiVersionMismatch) =>
        EventUtility.ConstructEvent(
            json,
            stripeSignatureHeader,
            secret,
            throwOnApiVersionMismatch: throwOnApiVersionMismatch);
}
