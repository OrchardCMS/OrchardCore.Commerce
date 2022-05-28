using Money;
using OrchardCore.Commerce.Abstractions;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Models;

public class OrderLineItem
{
    public int Quantity { get; set; }
    public string ProductSku { get; set; }
    public Amount UnitPrice { get; set; }
    public Amount LinePrice { get; set; }
    public IDictionary<string, IProductAttributeValue> Attributes { get; } =
        new Dictionary<string, IProductAttributeValue>();
}
