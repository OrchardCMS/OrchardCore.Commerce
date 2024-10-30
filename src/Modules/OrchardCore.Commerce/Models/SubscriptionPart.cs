using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrchardCore.Commerce.Models;

public class SubscriptionPart : ContentPart
{
    public TextField Status { get; set; } = new();
    public TextField IdInPaymentProvider { get; set; } = new();
    public TextField PaymentProviderName { get; set; } = new();
    public TextField UserId { get; set; } = new();
    public DateField StartDateUtc { get; set; } = new();
    public DateField EndDateUtc { get; set; } = new();

    public TextField SerializedMetadata { get; set; } = new();

    [JsonIgnore]
    public IDictionary<string, string> Metadata
    {
        get => JsonSerializer.Deserialize<IDictionary<string, string>>(SerializedMetadata.Text);
        set => SerializedMetadata.Text = JsonSerializer.Serialize(value);
    }
}
