using System.Collections.Generic;

namespace OrchardCore.Commerce.Payment.Stripe.Models;

public class StripeSessionDataSave
{
    public StripeSessionData StripeSessionData { get; set; }
    public IEnumerable<string> Errors { get; set; }
}
