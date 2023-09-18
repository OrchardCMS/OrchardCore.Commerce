using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.ViewModels;

public class TieredPricePartViewModel
{
    public string TieredValuesSerialized { get; set; }
    public string Currency { get; set; }

    [BindNever]
    public IEnumerable<ICurrency> Currencies { get; set; }

    [BindNever]
    public IDictionary<int, Amount> TieredPrices { get; private set; }

    [BindNever]
    public ContentItem ContentItem { get; set; }

    [BindNever]
    public TieredPricePart TieredPricePart { get; set; }

    public void InitializeTiers(
        IDictionary<int, Amount> tieredPrices,
        string currency)
    {
        TieredPrices = tieredPrices;

        var tieredValues = tieredPrices
            .Select(tier => new PriceTier { Quantity = tier.Key, UnitPrice = tier.Value.Value })
            .ToList();
        if (!tieredPrices.Any())
        {
            tieredValues.Add(new PriceTier { Quantity = 1, UnitPrice = null });
        }

        SerializeTieredValues(tieredValues);

        Currency = currency;
    }

    public IEnumerable<PriceTier> GetTieredValues() =>
        JArray.Parse(TieredValuesSerialized).ToObject<IEnumerable<PriceTier>>();

    public void SortTiersByQuantity() =>
        SerializeTieredValues(GetTieredValues().OrderBy(x => x.Quantity));

    private void SerializeTieredValues(IEnumerable<PriceTier> tieredValues) =>
        TieredValuesSerialized = JArray.FromObject(tieredValues.Select(x => new { x.Quantity, x.UnitPrice })).ToString();

}
