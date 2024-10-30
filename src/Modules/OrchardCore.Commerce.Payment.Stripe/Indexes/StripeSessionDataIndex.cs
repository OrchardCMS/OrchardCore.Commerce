using OrchardCore.Commerce.Payment.Stripe.Models;
using YesSql.Indexes;

namespace OrchardCore.Commerce.Payment.Stripe.Indexes;

public class StripeSessionDataIndex : MapIndex
{
    public string UserId { get; set; }
    public string StripeCustomerId { get; set; }
    public string StripeSessionId { get; set; }
    public string StripeSessionUrl { get; set; }
    public string StripeInvoiceId { get; set; }
    public string SerializedAdditionalData { get; set; }
}

public class StripeSessionDataIndexProvider : IndexProvider<StripeSessionData>
{
    public override void Describe(DescribeContext<StripeSessionData> context) =>
        context.For<StripeSessionDataIndex>()
            .Map(sessionData => new StripeSessionDataIndex
            {
                UserId = sessionData.UserId,
                StripeCustomerId = sessionData.StripeCustomerId,
                StripeSessionId = sessionData.StripeSessionId,
                StripeSessionUrl = sessionData.StripeSessionUrl,
                StripeInvoiceId = sessionData.StripeInvoiceId,
                SerializedAdditionalData = sessionData.SerializedAdditionalData,

            });
}
