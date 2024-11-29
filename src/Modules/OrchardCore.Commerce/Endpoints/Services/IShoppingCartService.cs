using OrchardCore.Commerce.Abstractions.ViewModels;
using OrchardCore.Commerce.ViewModels;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Endpoints;

/// <summary>
/// Add, remove, update shopping cart service.
/// </summary>
public interface IShoppingCartService
{
    /// <summary>
    /// Get shopping cart.
    /// </summary>
    /// <param name="shoppingCartId">Shopping cart Id.</param>
    /// <returns>A <see cref="ShoppingCartViewModel"/> that contains all the necessary information a shopping cart have.</returns>
    Task<ShoppingCartViewModel> GetAsync(string shoppingCartId = null);

    /// <summary>
    /// Update shopping cart.
    /// </summary>
    /// <param name="cart">Shopping cart.</param>
    /// <param name="token">Shopping cart token.</param>
    /// <param name="shoppingCartId">Shopping cart Id.</param>
    /// <returns>The result.</returns>
    Task<string> UpdateAsync(ShoppingCartUpdateModel cart, string token, string shoppingCartId = null);

    /// <summary>
    /// Add shopping cart line.
    /// </summary>
    /// <param name="line">Shopping cart line to be added.</param>
    /// <param name="token">Shopping cart token.</param>
    /// <param name="shoppingCartId">Shopping cart Id.</param>
    /// <returns>The result.</returns>
    Task<string> AddItemAsync(ShoppingCartLineUpdateModel line, string token, string shoppingCartId = null);

    /// <summary>
    /// Remove shopping cart line.
    /// </summary>
    /// <param name="line">Shopping cart line to be added.</param>
    /// <param name="shoppingCartId">Shopping cart Id.</param>
    /// <returns>The result.</returns>
    Task<string> RemoveLineAsync(ShoppingCartLineUpdateModel line, string shoppingCartId = null);
}
