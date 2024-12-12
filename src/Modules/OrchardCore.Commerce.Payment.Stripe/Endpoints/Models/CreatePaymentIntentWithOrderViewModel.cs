using OrchardCore.Commerce.Abstractions.Models;

namespace OrchardCore.Commerce.Payment.Stripe.Endpoints.Models;

public class CreatePaymentIntentWithOrderViewModel
{
    public string ShoppingCartId { get; set; }
    public OrderPart OrderPart { get; set; }
}
