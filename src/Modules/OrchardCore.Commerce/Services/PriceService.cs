using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

/// <summary>
/// A price service that asks all available price providers to add prices to a list of shopping cart items.
/// </summary>
public class PriceService : IPriceService
{
    private readonly IEnumerable<IPriceProvider> _providers;

    public PriceService(IEnumerable<IPriceProvider> priceProviders) => _providers = priceProviders;

    public async Task<IList<ShoppingCartItem>> AddPricesAsync(IList<ShoppingCartItem> items)
    {
        var providers = await _providers
            .OrderBy(provider => provider.Order)
            .WhereAsync(provider => provider.IsApplicableAsync(items));

        foreach (var priceProvider in providers)
        {
            var result = await priceProvider.AddPricesAsync(items);
            items = result as IList<ShoppingCartItem> ?? result.ToList();
        }

        return items;
    }
}
