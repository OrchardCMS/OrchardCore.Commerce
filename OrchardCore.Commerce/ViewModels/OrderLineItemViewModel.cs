using Microsoft.AspNetCore.Routing;
using Money;
using OrchardCore.Commerce.Abstractions;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OrchardCore.Commerce.ViewModels;

[SuppressMessage(
    "Usage",
    "CA2227:Collection properties should be read only",
    Justification = "We don't want to mess with the RouteValueDictionary, also it's just a view-model so it's safe.")]
public class OrderLineItemViewModel
{
    public int Quantity { get; set; }
    public string ProductSku { get; set; }
    public string ProductName { get; set; }

    public RouteValueDictionary ProductRouteValues { get; set; }
    public string ProductImageUrl { get; set; }
    public Amount UnitPrice { get; set; }
    public Amount LinePrice { get; set; }
    public IDictionary<string, IProductAttributeValue> Attributes { get; set; }
}
