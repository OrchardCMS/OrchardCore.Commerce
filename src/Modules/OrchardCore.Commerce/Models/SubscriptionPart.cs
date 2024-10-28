using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Models;

public class SubscriptionPart : ContentPart
{
    public TextField Status { get; set; } = new();
    public TextField IdInPaymentProvider { get; set; } = new();
    public TextField PaymentProviderName { get; set; } = new();
    public TextField UserId { get; set; } = new();
    public DateField StartDateUtc { get; set; } = new();
    public DateField EndDateUtc { get; set; } = new();
}
