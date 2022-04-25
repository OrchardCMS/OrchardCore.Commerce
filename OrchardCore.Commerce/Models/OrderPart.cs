using OrchardCore.Commerce.Abstractions;
using OrchardCore.ContentManagement;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Models;

public class OrderPart : ContentPart
{
    /// <summary>
    /// Gets the line items in this order.
    /// </summary>
    public IList<OrderLineItem> LineItems { get; } = new List<OrderLineItem>();

    /// <summary>
    /// Gets additional costs such as taxes and shipping.
    /// </summary>
    public IList<OrderAdditionalCost> AdditionalCosts { get; } = new List<OrderAdditionalCost>();

    /// <summary>
    /// Gets the amounts charged on this order. Typically a single credit card charge.
    /// </summary>
    public IList<IPayment> Charges { get; } = new List<IPayment>();
}
