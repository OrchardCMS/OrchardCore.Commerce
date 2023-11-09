using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.MoneyDataType;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// A service that can add prices to a set of shopping cart items.
/// </summary>
public interface IPriceService
{
    /// <summary>
    /// Adds prices harvested from all price providers to shopping cart items, in order.
    /// </summary>
    /// <param name="items">The quantities and products to which prices must be added.</param>
    Task<IList<ShoppingCartItem>> AddPricesAsync(IList<ShoppingCartItem> items);

    /// <summary>
    /// Returns a price from the provided collection of <paramref name="prices"/> using the default <see
    /// cref="IPriceSelectionStrategy"/>.
    /// </summary>
    Amount SelectPrice(IEnumerable<PrioritizedPrice> prices);
}
