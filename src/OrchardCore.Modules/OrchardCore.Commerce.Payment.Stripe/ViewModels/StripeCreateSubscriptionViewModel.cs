using OrchardCore.Commerce.Abstractions.Models;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Payment.Stripe.ViewModels;

public class StripeCreateSubscriptionViewModel
{
    public string ShoppingCartId { get; set; }
    public string CustomerId { get; set; }
    public IList<string> PriceIds { get; } = [];

    public OrderPart OrderPart { get; set; }
}
