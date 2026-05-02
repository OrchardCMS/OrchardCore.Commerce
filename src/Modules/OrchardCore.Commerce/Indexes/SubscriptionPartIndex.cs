using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using System;
using System.Text.Json;
using YesSql.Indexes;

namespace OrchardCore.Commerce.Indexes;

public class SubscriptionPartIndex : MapIndex
{
    public string Status { get; set; }
    public string IdInPaymentProvider { get; set; }
    public string PaymentProviderName { get; set; }
    public string UserId { get; set; }
    public string SerializedMetadata { get; set; }
    public DateTime StartDateUtc { get; set; }
    public DateTime EndDateUtc { get; set; }
}

/// <summary>
/// Creates an index of content items (products in this case) by SKU.
/// </summary>
public class SubscriptionPartIndexProvider : IndexProvider<ContentItem>
{
    // Notice that ContentItem is what we are describing the provider for not the part.
    public override void Describe(DescribeContext<ContentItem> context) =>
        context.For<SubscriptionPartIndex>()
            .When(contentItem => contentItem.Has<SubscriptionPart>())
            .Map(contentItem =>
            {
                var subscriptionPart = contentItem.As<SubscriptionPart>();

                return new SubscriptionPartIndex
                {
                    Status = subscriptionPart.Status.Text,
                    IdInPaymentProvider = subscriptionPart.IdInPaymentProvider.Text,
                    PaymentProviderName = subscriptionPart.PaymentProviderName.Text,
                    UserId = subscriptionPart.UserId.Text,
                    StartDateUtc = subscriptionPart.StartDateUtc.Value!.Value,
                    EndDateUtc = subscriptionPart.EndDateUtc.Value!.Value,
                    SerializedMetadata = JsonSerializer.Serialize(subscriptionPart.Metadata),
                };
            });
}
