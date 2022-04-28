using Money;
using OrchardCore.Commerce.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.ViewModels;

public class ShoppingCartLineViewModel
{
    public IDictionary<string, IProductAttributeValue> Attributes { get; }
    public int Quantity { get; set; }
    public string ProductSku { get; set; }
    public string ProductName { get; set; }
    public string ProductUrl { get; set; }
    public string ProductImageUrl { get; set; }
    public Amount UnitPrice { get; set; }
    public Amount LinePrice { get; set; }

    public ShoppingCartLineViewModel(IDictionary<string, IProductAttributeValue> attributes = null) =>
        Attributes = attributes ?? new Dictionary<string, IProductAttributeValue>();

    public static bool IsSameProductAs(ShoppingCartLineViewModel line, ShoppingCartLineViewModel other) =>
        other.ProductSku == line.ProductSku &&
        line.Attributes?.Count == other.Attributes.Count &&
        !line.Attributes.Except(other.Attributes).Any();
}
