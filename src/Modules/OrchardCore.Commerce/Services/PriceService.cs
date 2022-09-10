using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

/// <summary>
/// A price service that asks all available price providers to add prices to a list of shopping cart items.
/// </summary>
public class PriceService : IPriceService
{
    private readonly IEnumerable<IPriceProvider> _priceProviders;
    private readonly IPriceSelectionStrategy _priceSelectionStrategy;

    public PriceService(
        IEnumerable<IPriceProvider> priceProviders,
        IPriceSelectionStrategy priceSelectionStrategy)
    {
        _priceProviders = priceProviders;
        _priceSelectionStrategy = priceSelectionStrategy;
    }

    public async Task<IList<ShoppingCartItem>> AddPricesAsync(IList<ShoppingCartItem> items)
    {
        var providers = await _priceProviders
            .OrderBy(provider => provider.Order)
            .WhereAsync(provider => provider.IsApplicableAsync(items));

        foreach (var priceProvider in providers)
        {
            var result = await priceProvider.UpdateAsync(items);
            items = result.AsList();
        }

        return items;
    }

    public Amount SelectPrice(IEnumerable<PrioritizedPrice> prices) =>
        _priceSelectionStrategy.SelectPrice(prices);
}
