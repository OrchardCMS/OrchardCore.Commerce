using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
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
        var priceProviders = _priceProviders.OrderBy(provider => provider.Order).ToList();

        // First let's check for providers that are applicable for all items in the list.
        var fullyApplicableProviders = await priceProviders.WhereAsync(provider => provider.IsApplicableAsync(items));
        foreach (var provider in fullyApplicableProviders) items = await provider.UpdateAsync(items);

        // If all applicable providers were used, then there is nothing left to do.
        var remainingProviders = priceProviders.Except(fullyApplicableProviders).ToList();
        if (!remainingProviders.Any()) return items;

        // If only a mixture of providers can work, then we handle each applicable item together. By storing the
        // original indexes here, we ensure that the results will be in the same order as the input items.
        var unhandled = items.Select((item, index) => (Item: item, Index: index)).ToList();
        var handled = new List<(ShoppingCartItem Item, int Index)>();

        // Take out subsets where items are individually applicable to the remaining providers.
        foreach (var provider in remainingProviders)
        {
            var applicable = await unhandled.WhereAsync(pair => provider.IsApplicableAsync(new[] { pair.Item }));
            if (!applicable.Any()) continue;

            unhandled.RemoveAll(applicable.Contains);

            var providerResult = await provider.UpdateAsync(applicable.Select(pair => pair.Item).ToList());
            handled.AddRange(providerResult.Select((item, index) => (item, applicable[index].Index)));
        }

        // If any items were handled then rebuild the list, otherwise cut it short here.
        if (!handled.Any()) return items;
        if (unhandled.Any()) handled.AddRange(unhandled);

        return handled
            .OrderBy(pair => pair.Index)
            .Select(pair => pair.Item)
            .ToList();
    }

    public Amount SelectPrice(IEnumerable<PrioritizedPrice> prices) =>
        _priceSelectionStrategy.SelectPrice(prices);
}
