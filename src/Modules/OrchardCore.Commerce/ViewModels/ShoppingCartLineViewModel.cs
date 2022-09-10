using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.ViewModels;

public class ShoppingCartLineViewModel
{
    public IDictionary<string, IProductAttributeValue> Attributes { get; }
    public int Quantity { get; set; }
    public string ProductSku { get; set; }
    public string ProductName { get; set; }
    public string ProductImageUrl { get; set; }
    public Amount UnitPrice { get; set; }
    public Amount LinePrice { get; set; }

    public IDictionary<string, JToken> AdditionalData { get; } = new Dictionary<string, JToken>();

    [BindNever]
    public ProductPart Product { get; set; }

    public ShoppingCartLineViewModel(IDictionary<string, IProductAttributeValue> attributes = null) =>
        Attributes = attributes ?? new Dictionary<string, IProductAttributeValue>();

    public static bool IsSameProductAs(ShoppingCartLineViewModel line, ShoppingCartLineViewModel other) =>
        other.ProductSku == line.ProductSku &&
        line.Attributes.Count == other.Attributes.Count &&
        !line.Attributes.Except(other.Attributes).Any();
}
