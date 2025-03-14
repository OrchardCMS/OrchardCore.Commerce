using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Payment.Stripe.Models;
using OrchardCore.Html.Models;
using OrchardCore.Title.Models;

namespace OrchardCore.Commerce.Payment.Stripe.Endpoints.Models;

public class OrderViewModel
{
    public TitlePart TitlePart { get; set; }
    public HtmlBodyPart HtmlBodyPart { get; set; }
    public OrderPart OrderPart { get; set; }
    public StripePaymentPart StripePaymentPart { get; set; }
    public string PaymentIntentId { get; set; }
}
