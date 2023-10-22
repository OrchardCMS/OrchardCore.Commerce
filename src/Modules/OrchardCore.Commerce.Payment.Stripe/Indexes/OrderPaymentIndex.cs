using OrchardCore.Commerce.Models;
using YesSql.Indexes;

namespace OrchardCore.Commerce.Indexes;

public class OrderPaymentIndex : MapIndex
{
    public string PaymentIntentId { get; set; }
    public string OrderId { get; set; }
}

/// <summary>
/// Creates an index of content items (products in this case) by SKU.
/// </summary>
public class OrderPaymentIndexProvider : IndexProvider<OrderPayment>
{
    public override void Describe(DescribeContext<OrderPayment> context) =>
        context.For<OrderPaymentIndex>()
            .Map(orderPayment => new OrderPaymentIndex
            {
                OrderId = orderPayment.OrderId,
                PaymentIntentId = orderPayment.PaymentIntentId,
            });
}
