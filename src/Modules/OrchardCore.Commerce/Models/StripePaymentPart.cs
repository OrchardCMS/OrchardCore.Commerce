using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Models;

public class StripePaymentPart : ContentPart
{
    public TextField PaymentMethodId { get; set; } = new();
    public TextField PaymentIntentId { get; set; } = new();
}
