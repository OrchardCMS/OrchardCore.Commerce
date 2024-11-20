using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Commerce.Payment.Stripe.ViewModels;

public class StripeGetSubscriptionViewModel
{
    [Required]
    public string SubscriptionId { get; set; }
}
