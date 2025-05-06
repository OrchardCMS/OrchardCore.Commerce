using OrchardCore.Commerce.Abstractions.Models;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Service that provides a way to retain shopping cart information.
/// </summary>
/// <remarks><para>
/// When deriving a custom implementation, please inherit from <c>ShoppingCartPersistenceBase</c> in the
/// <c>OrchardCore.Commerce</c> project to retain event handling and dependency injection scope level caching.
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
    /// Saves the provided <see cref="ShoppingCart"/> <paramref name="items"/>.
    /// </summary>
    Task StoreAsync(ShoppingCart items);

    /// <summary>
    /// Remove a <see cref="ShoppingCart"/> identified by <paramref name="shoppingCartId"/>.
    /// </summary>
    /// <param name="shoppingCartId">
    /// The name used to identify the shopping cart. <see langword="null"/> refers to the default shopping cart.
    /// </param>
    Task RemoveAsync(string shoppingCartId);
}

public static class ShoppingCartPersistenceExtensions
{
    public static Task StoreAsync(this IShoppingCartPersistence service, ShoppingCart items, string shoppingCartId)
    {
        items.Id = shoppingCartId ?? items.Id;
        return service.StoreAsync(items);
    }
}
