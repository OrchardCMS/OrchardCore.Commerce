using System.Collections.Generic;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Models;

public class OrderPart : ContentPart
{
    /// <summary>
    /// Gets or sets the line items in this order.
    /// </summary>
    public IList<OrderLineItem> LineItems { get; set; }

    /// <summary>
    /// Gets or sets additional costs such as taxes and shipping.
    /// </summary>
    public IList<OrderAdditionalCost> AdditionalCosts { get; set; }

    /// <summary>
    /// Gets or sets the amounts charged on this order. Typically a single credit card charge.
    /// </summary>
    public IList<IPayment> Charges { get; set; }
}
