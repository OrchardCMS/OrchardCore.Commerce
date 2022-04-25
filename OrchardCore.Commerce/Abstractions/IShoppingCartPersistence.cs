using OrchardCore.Commerce.Models;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Service that provides a way to retain shopping cart information.
/// </summary>
public interface IShoppingCartPersistence
{
    /// <summary>
    /// Returns a <see cref="ShoppingCart"/> identified by <paramref name="shoppingCartId"/>.
    /// </summary>
    Task<ShoppingCart> RetrieveAsync(string shoppingCartId = null);

    /// <summary>
    /// Saves a shopping card by a given ID.
    /// </summary>
    Task StoreAsync(ShoppingCart items, string shoppingCartId = null);

    /// <summary>
    /// Generates a shopping cart ID from <paramref name="shoppingCartId"/> that's unique to this persistence implementation.
    /// </summary>
    string GetUniqueCartId(string shoppingCartId);
}
