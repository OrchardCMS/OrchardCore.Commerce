using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.ContentManagement;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Models;

public class TieredPricePart : ContentPart
{
    public IDictionary<int, Amount> TieredPrices { get; } = new Dictionary<int, Amount>();
}
