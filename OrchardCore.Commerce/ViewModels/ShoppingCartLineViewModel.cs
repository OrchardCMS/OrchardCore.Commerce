using System.Collections.Generic;
using Money;
using OrchardCore.Commerce.Abstractions;
using System.Linq;

namespace OrchardCore.Commerce.ViewModels;

public class ShoppingCartLineViewModel
{
    public int Quantity { get; set; }
    public string ProductSku { get; set; }
    public string ProductName { get; set; }
    public string ProductUrl { get; set; }
    public string ProductImageUrl { get; set; }
    public Amount UnitPrice { get; set; }
    public Amount LinePrice { get; set; }
    public IDictionary<string, IProductAttributeValue> Attributes { get; }

    public ShoppingCartLineViewModel(IDictionary<string, IProductAttributeValue> attributes = null) =>
        Attributes = attributes ?? new Dictionary<string, IProductAttributeValue>();

    public static bool IsSameProductAs(ShoppingCartLineViewModel line, ShoppingCartLineViewModel other) =>
        other.ProductSku == line.ProductSku
        && (
            ((line.Attributes is null || line.Attributes.Count == 0) && (other.Attributes is null || other.Attributes.Count == 0))
            || (line.Attributes?.Count == other.Attributes.Count && !line.Attributes.Except(other.Attributes).Any())
        );
}
