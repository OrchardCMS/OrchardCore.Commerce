using OrchardCore.Commerce.AddressDataType;
using Stripe.Checkout;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Payment.Stripe.Endpoints.Models;

public class SubscriptionCheckoutEndpointViewModel
{
    public string SuccessUrl { get; set; }
    public string CancelUrl { get; set; }
    public IList<SessionLineItemOptions> SessionLineItemOptions { get; set; } = new List<SessionLineItemOptions>();
    public PaymentMode PaymentMode { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public Address BillingAddress { get; set; }
    public Address ShippingAddress { get; set; }
}
