using Money;
using OrchardCore.Commerce.Abstractions;
using System;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Models;

public class OrderLineItem
{
    public int Quantity { get; set; }
    public string ProductSku { get; set; }
    public Amount UnitPrice { get; set; }
    public Amount LinePrice { get; set; }
    public ISet<IProductAttributeValue> Attributes { get; }

    public OrderLineItem(
    int quantity,
    string productSku,
    Amount unitPrice,
    Amount linePrice,
    IEnumerable<IProductAttributeValue> attributes = null)
    {
        ArgumentNullException.ThrowIfNull(productSku);
        if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity));

        Quantity = quantity;
        ProductSku = productSku;
        UnitPrice = unitPrice;
        LinePrice = linePrice;
        Attributes = attributes is null
            ? new HashSet<IProductAttributeValue>()
            : new HashSet<IProductAttributeValue>(attributes);
    }
}
