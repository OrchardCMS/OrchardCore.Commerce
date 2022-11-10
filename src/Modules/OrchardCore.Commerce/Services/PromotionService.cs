using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

/// <summary>
/// A promotion service that asks all available promotion providers to apply promotions to a list of shopping cart
/// items.
/// </summary>
public class PromotionService : IPromotionService
{
    private readonly IEnumerable<IPromotionProvider> _promotionProviders;

    public PromotionService(IEnumerable<IPromotionProvider> promotionProviders) =>
        _promotionProviders = promotionProviders;

    public async Task<IList<ShoppingCartItem>> AddPromotionsAsync(IList<ShoppingCartItem> items)
    {
        var providers = await _promotionProviders
            .OrderBy(provider => provider.Order)
            .WhereAsync(provider => provider.IsApplicableAsync(items));

        foreach (var priceProvider in providers)
        {
            var result = await priceProvider.UpdateAsync(items);
            items = result.AsList();
        }

        return items;
    }
}
