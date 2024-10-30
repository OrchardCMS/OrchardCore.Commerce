using OrchardCore.Commerce.AddressDataType;
using Stripe.Checkout;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Payment.Stripe.Endpoints.Models;

public class SubscriptionCheckoutEndpointViewModel
{
    public string SuccessUrl { get; set; }
    public string CancelUrl { get; set; }

    // This is an API model so we don't need to make it read-only.
#pragma warning disable CA2227 // CA2227: Change 'SessionLineItemOptions' to be read-only by removing the property setter
    public IList<SessionLineItemOptions> SessionLineItemOptions { get; set; } = new List<SessionLineItemOptions>();
#pragma warning restore CA2227
    public PaymentMode PaymentMode { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public Address BillingAddress { get; set; }
    public Address ShippingAddress { get; set; }
}
