namespace OrchardCore.Commerce.Payment.Stripe.Models;

public class OrderPayment
{
    public string PaymentIntentId { get; set; }
    public string OrderId { get; set; }
}
