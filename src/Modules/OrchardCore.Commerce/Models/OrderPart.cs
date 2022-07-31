using Newtonsoft.Json;
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

    // This is a temporary solution, it needs to be reworked in the future!
#pragma warning disable CA2326 // Do not use TypeNameHandling values other than None
#pragma warning disable SCS0028 // TypeNameHandling is set to the other value than 'None'. It may lead to deserialization vulnerability.
    [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)]
#pragma warning restore SCS0028 // TypeNameHandling is set to the other value than 'None'. It may lead to deserialization vulnerability.
#pragma warning restore CA2326 // Do not use TypeNameHandling values other than None
    public IList<IPayment> Charges { get; } = new List<IPayment>();
}
