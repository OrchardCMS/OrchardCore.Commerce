using OrchardCore.Commerce.Abstractions.Models;
using Stripe.Checkout;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Payment.Stripe.Endpoints.Models;

public class SubscriptionCheckoutEndpointViewModel
{
    public string SuccessUrl { get; set; }
    public string CancelUrl { get; set; }
    public string CustomerId { get; set; }
    public IList<SessionLineItemOptions> SessionLineItemOptions { get; set; } = new List<SessionLineItemOptions>();
    public PaymentMode PaymentMode { get; set; }
    public OrderPart Information { get; set; }
}
