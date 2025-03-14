using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Models;

public class TieredPricePart : ContentPart
{
    public Amount DefaultPrice { get; set; }
    public IList<PriceTier> PriceTiers { get; } = [];

    public Amount GetPriceForQuantity(IMoneyService moneyService, int quantity)
    {
        if (PriceTiers.Any(tier => tier.Quantity <= quantity))
        {
            // Get the tiered price for the quantity (or the closest one).
            var closestTier = PriceTiers
                .OrderByDescending(tier => tier.Quantity)
                .FirstOrDefault(tier => tier.Quantity <= quantity);

            if (closestTier?.UnitPrice != null)
            {
                return moneyService.Create(
                    closestTier.UnitPrice.Value,
                    DefaultPrice.Currency.CurrencyIsoCode);
            }
        }

        return DefaultPrice;
    }
}
