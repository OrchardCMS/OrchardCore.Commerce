using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.MoneyDataType;
using System;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Models;

public class OrderLineItem
{
    public int Quantity { get; set; }
    public string ProductSku { get; set; }
    public string FullSku { get; set; }
    public Amount UnitPrice { get; set; }
    public Amount LinePrice { get; set; }
    public string ContentItemVersion { get; set; }
    public ISet<IProductAttributeValue> Attributes { get; }
    public IDictionary<string, string> SelectedTextAttributes { get; } = new Dictionary<string, string>();
    public IDictionary<string, string> SelectedBooleanAttributes { get; } = new Dictionary<string, string>();
    public IDictionary<string, string> SelectedNumericAttributes { get; } = new Dictionary<string, string>();

    // These are necessary.
#pragma warning disable S107 // Methods should not have too many parameters
    public OrderLineItem(
        int quantity,
        string productSku,
        string fullSku,
        Amount unitPrice,
        Amount linePrice,
        string contentItemVersion,
        IEnumerable<IProductAttributeValue> attributes = null,
        IDictionary<string, string> selectedTextAttributes = null,
        IDictionary<string, string> selectedBooleanAttributes = null,
        IDictionary<string, string> selectedNumericAttributes = null)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        ArgumentNullException.ThrowIfNull(productSku);
        if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity));

        Quantity = quantity;
        ProductSku = productSku;
        FullSku = fullSku;
        UnitPrice = unitPrice;
        LinePrice = linePrice;
        ContentItemVersion = contentItemVersion;
        Attributes = attributes is null
            ? new HashSet<IProductAttributeValue>()
            : new HashSet<IProductAttributeValue>(attributes);
        SelectedTextAttributes.AddRange(selectedTextAttributes);
        SelectedBooleanAttributes.AddRange(selectedBooleanAttributes);
        SelectedNumericAttributes.AddRange(selectedNumericAttributes);
    }
}
