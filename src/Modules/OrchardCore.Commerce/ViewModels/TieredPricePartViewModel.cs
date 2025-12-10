using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace OrchardCore.Commerce.ViewModels;

public class TieredPricePartViewModel
{
    public decimal? DefaultPrice { get; set; }
    public string TieredValuesSerialized { get; set; }
    public string Currency { get; set; }

    [BindNever]
    public IEnumerable<ICurrency> Currencies { get; set; }

    [BindNever]
    public IEnumerable<PriceTier> Tiers { get; private set; }

    [BindNever]
    public ContentItem ContentItem { get; set; }

    [BindNever]
    public TieredPricePart TieredPricePart { get; set; }

    public void InitializeTiers(
        Amount defaultPrice,
        IEnumerable<PriceTier> tiers,
        string currency)
    {
        DefaultPrice = defaultPrice.Value;
        Tiers = tiers.OrderBy(tier => tier.Quantity);
        TieredValuesSerialized = JArray.FromObject(tiers)?.ToJsonString();

        Currency = currency;
    }

    public IEnumerable<PriceTier> DeserializePriceTiers() =>
        JArray.Parse(TieredValuesSerialized).ToObject<IEnumerable<PriceTier>>().OrderBy(tier => tier.Quantity);
}
