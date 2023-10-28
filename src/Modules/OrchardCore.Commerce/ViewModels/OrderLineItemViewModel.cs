using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Settings;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OrchardCore.Commerce.ViewModels;

[SuppressMessage(
    "Usage",
    "CA2227:Collection properties should be read only",
    Justification = "We don't want to mess with the RouteValueDictionary, also it's just a view-model so it's safe.")]
public class OrderLineItemViewModel : ILineItem
{
    [BindNever]
    public ProductPart ProductPart { get; set; }
    public int Quantity { get; set; }
    public string ProductSku { get; set; }
    public string ProductFullSku { get; set; }
    public string ProductName { get; set; }

    public RouteValueDictionary ProductRouteValues { get; set; }
    public string ProductImageUrl { get; set; }
    public decimal UnitPriceValue { get; set; }
    public string UnitPriceCurrencyIsoCode { get; set; }
    public Amount UnitPrice { get; set; }
    public Amount LinePrice { get; set; }
    public ISet<IProductAttributeValue> Attributes { get; set; }
    public IDictionary<string, IDictionary<string, string>> SelectedAttributes { get; set; } =
        new Dictionary<string, IDictionary<string, string>>();
    public IDictionary<string, IDictionary<string, List<string>>> AvailableTextAttributes { get; set; } =
        new Dictionary<string, IDictionary<string, List<string>>>();
    public IDictionary<string, List<string>> AvailableBooleanAttributes { get; set; } = new Dictionary<string, List<string>>();
    public IDictionary<string, List<string>> AvailableNumericAttributes { get; set; } = new Dictionary<string, List<string>>();
    public IDictionary<string, IDictionary<string, NumericProductAttributeFieldSettings>> NumericAttributeSettings { get; set; } =
        new Dictionary<string, IDictionary<string, NumericProductAttributeFieldSettings>>();
}
