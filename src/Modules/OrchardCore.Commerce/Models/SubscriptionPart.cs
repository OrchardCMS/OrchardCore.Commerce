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
    public DateTimeField StartDateUtc { get; set; } = new();
    public DateTimeField EndDateUtc { get; set; } = new();

    public TextField SerializedMetadata { get; set; } = new();

    [JsonIgnore]
    // We are not directly setting the metadata field, but we are serializing it to a text field
#pragma warning disable CA2227 // CA2227: Change 'Metadata' to be read-only by removing the property setter
    public IDictionary<string, string> Metadata
#pragma warning restore CA2227
    {
        get => JsonSerializer.Deserialize<IDictionary<string, string>>(SerializedMetadata.Text);
        set => SerializedMetadata.Text = JsonSerializer.Serialize(value);
    }
}
