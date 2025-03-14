using Stripe;

namespace OrchardCore.Commerce.Payment.Stripe.ViewModels;

public class StripeCustomerViewModel
{
    public CustomerCreateOptions CustomerCreateOptions { get; set; }
}
