using OrchardCore.Commerce.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// A service that can apply promotions to a set of shopping cart items.
/// </summary>
public interface IPromotionService
{
    /// <summary>
    /// Applies promotions harvested from all promotion providers to shopping cart items, in order.
    /// </summary>
    /// <param name="items">The quantities and products to which prices must be added.</param>
    Task<IList<ShoppingCartItem>> AddPromotionsAsync(IList<ShoppingCartItem> items);
}
