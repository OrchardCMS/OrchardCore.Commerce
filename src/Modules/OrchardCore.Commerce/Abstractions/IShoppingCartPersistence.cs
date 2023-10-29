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
    /// <param name="shoppingCartId">
    /// The name used to identify the shopping cart. <see langword="null"/> refers to the default shopping cart.
    /// </param>
    Task<ShoppingCart> RetrieveAsync(string shoppingCartId);

    /// <summary>
    /// Saves a shopping cart by a given ID.
    /// </summary>
    Task StoreAsync(ShoppingCart items);
}

public static class ShoppingCartPersistenceExtensions
{
    public static Task StoreAsync(this IShoppingCartPersistence service, ShoppingCart items, string shoppingCartId)
    {
        items.Id = shoppingCartId ?? items.Id;
        return service.StoreAsync(items);
    }
}
