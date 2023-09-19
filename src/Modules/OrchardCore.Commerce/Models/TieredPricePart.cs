using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Models;

public class TieredPricePart : ContentPart
{
    public Amount DefaultPrice { get; set; }
    public IList<PriceTier> PriceTiers { get; private set; } = new List<PriceTier>();

    public Amount GetPriceForQuantity(IMoneyService moneyService, int quantity)
    {
        if (PriceTiers is { } tiers && tiers.Any(tier => tier.Quantity <= quantity))
        {
            // Get the tiered price for the quantity (or the closest one).
            var closestTier = tiers
                .OrderByDescending(x => x.Quantity)
                .FirstOrDefault(x => x.Quantity <= quantity);
            return moneyService.Create(closestTier.UnitPrice ?? 0, DefaultPrice.Currency.CurrencyIsoCode);
        }

        return DefaultPrice;
    }
}
