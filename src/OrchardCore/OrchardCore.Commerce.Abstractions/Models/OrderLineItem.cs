using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.MoneyDataType;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OrchardCore.Commerce.Abstractions.Models;

public class OrderLineItem
{
    public int Quantity { get; set; }
    public string ProductSku { get; set; }
    public string FullSku { get; set; }
    public Amount UnitPrice { get; set; }
    public Amount LinePrice { get; set; }
    public string ContentItemVersion { get; set; }
    public ISet<IProductAttributeValue> Attributes { get; init; }
    public IDictionary<string, IDictionary<string, string>> SelectedAttributes { get; init; } =
        new Dictionary<string, IDictionary<string, string>>();

    [JsonConstructor]
    private OrderLineItem()
    {
    }

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
        IDictionary<string, IDictionary<string, string>> selectedAttributes = null)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        ArgumentNullException.ThrowIfNull(productSku);
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);

        Quantity = quantity;
        ProductSku = productSku;
        FullSku = fullSku;
        UnitPrice = unitPrice;
        LinePrice = linePrice;
        ContentItemVersion = contentItemVersion;
        Attributes = attributes is null
            ? []
            : new HashSet<IProductAttributeValue>(attributes);
        SelectedAttributes.AddRange(selectedAttributes);
    }
}
