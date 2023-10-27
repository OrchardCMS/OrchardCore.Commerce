using OrchardCore.Commerce.Payment.Abstractions;

namespace OrchardCore.Commerce.Services;

public class StripePaymentProvider : IPaymentProvider
{

    public string Name => "Stripe";
}
