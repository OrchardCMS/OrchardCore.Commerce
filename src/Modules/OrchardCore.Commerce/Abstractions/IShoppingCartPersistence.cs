using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Services;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Service that provides a way to retain shopping cart information.
/// </summary>
/// <remarks><para>
/// When deriving a custom implementation, please inherit from <see cref="ShoppingCartPersistenceBase"/> to retain event
/// handling and dependency injection scope level caching.
/// </para></remarks>
public interface IShoppingCartPersistence
{
    /// <summary>
    /// Returns a <see cref="ShoppingCart"/> identified by <paramref name="shoppingCartId"/>.
    /// </summary>
    Task<ShoppingCart> RetrieveAsync(string shoppingCartId = null);

    /// <summary>
    /// Saves a shopping cart by a given ID.
    /// </summary>
    Task StoreAsync(ShoppingCart items, string shoppingCartId = null);
}
