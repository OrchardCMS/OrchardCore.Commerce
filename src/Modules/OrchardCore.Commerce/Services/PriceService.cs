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
        var priceProviders = _priceProviders.OrderBy(provider => provider.Order).ToList();

        // First let's check if a single provider can handle all solutions. In case some provider require the whole list
        // as context to work.
        foreach (var provider in priceProviders)
        {
            if (await provider.IsApplicableAsync(items))
            {
                return await provider.UpdateAsync(items);
            }
        }

        // If only a mixture of providers can work, then we handle each applicable item together.
        var original = items.ToList();
        var handled = new List<ShoppingCartItem>();

        foreach (var provider in priceProviders)
        {
            var applicable = await items.WhereAsync(item => provider.IsApplicableAsync(new[] { item }));

            original.RemoveAll(item => applicable.Contains(item));
            handled.AddRange(await provider.UpdateAsync(applicable));
        }

        if (original.Any()) handled.AddRange(original);

        return handled;
    }

    public Amount SelectPrice(IEnumerable<PrioritizedPrice> prices) =>
        _priceSelectionStrategy.SelectPrice(prices);
}
