using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using System.Collections.Generic;
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

    public Task<IList<ShoppingCartItem>> AddPricesAsync(IList<ShoppingCartItem> items) =>
        _priceProviders.UpdateWithFirstApplicableProviderAsync(items);

    public Amount SelectPrice(IEnumerable<PrioritizedPrice> prices) =>
        _priceSelectionStrategy.SelectPrice(prices);
}
