using OrchardCore.Commerce.Abstractions;
using OrchardCore.ContentManagement;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Models;

public class OrderPart : ContentPart
{
    /// <summary>
    /// Gets the order's line items.
    /// </summary>
    public IList<OrderLineItem> LineItems { get; } = new List<OrderLineItem>();

    /// <summary>
    /// Gets additional costs that don't belong to an <see cref="OrderLineItem"/>, such as taxes and shipping.
    /// </summary>
    public IList<OrderAdditionalCost> AdditionalCosts { get; } = new List<OrderAdditionalCost>();

    /// <summary>
    /// Gets the amounts charged for this order. Typically a single credit card charge.
    /// </summary>
    public IList<IPayment> Charges { get; } = new List<IPayment>();
}
