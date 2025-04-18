using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Tests.Fakes;

public class FakeCartStorage : IShoppingCartPersistence
{
    private readonly Dictionary<string, ShoppingCart> _carts = [];

    public FakeCartStorage(ShoppingCart cart = null, string cartId = null) =>
        _carts[cartId ?? string.Empty] = cart != null
            ? new ShoppingCart(cart.Items)
            : new ShoppingCart();

    public Task<ShoppingCart> RetrieveAsync(string shoppingCartId)
    {
        if (!_carts.TryGetValue(shoppingCartId ?? string.Empty, out var cart))
        {
            cart = new ShoppingCart { Id = shoppingCartId };
            _carts.Add(shoppingCartId ?? string.Empty, cart);
        }

        return Task.FromResult(cart);
    }

    public Task StoreAsync(ShoppingCart items)
    {
        _carts[items.Id ?? string.Empty] = new ShoppingCart(items.Items);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string shoppingCartId)
    {
        _carts.Remove(shoppingCartId);
        return Task.CompletedTask;
    }
}
