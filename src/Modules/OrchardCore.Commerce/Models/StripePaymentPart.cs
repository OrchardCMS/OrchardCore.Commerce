using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Models;

public class StripePaymentPart : ContentPart
{
    public TextField PaymentIntentId { get; set; } = new();
}
