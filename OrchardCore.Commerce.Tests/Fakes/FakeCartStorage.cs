using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Tests.Fakes;

public class FakeCartStorage : IShoppingCartPersistence
{
    private readonly Dictionary<string, ShoppingCart> _carts = new();

    public FakeCartStorage(ShoppingCart cart = null, string cartId = null) =>
        _carts[cartId ?? string.Empty] = cart != null
            ? new ShoppingCart(cart.Items)
            : new ShoppingCart();

    public string GetUniqueCartId(string shoppingCartId)
        => Guid.NewGuid().ToString();

    public Task<ShoppingCart> RetrieveAsync(string shoppingCartId = null)
    {
        if (!_carts.TryGetValue(shoppingCartId ?? string.Empty, out var cart))
        {
            cart = new ShoppingCart();
            _carts.Add(shoppingCartId ?? string.Empty, cart);
        }

        return Task.FromResult(cart);
    }

    public Task StoreAsync(ShoppingCart items, string shoppingCartId = null)
    {
        _carts[shoppingCartId ?? string.Empty] = new ShoppingCart(items.Items);
        return Task.CompletedTask;
    }
}
