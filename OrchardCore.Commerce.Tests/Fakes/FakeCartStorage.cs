using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Tests.Fakes
{
    public class FakeCartStorage : IShoppingCartPersistence
    {
        private Dictionary<string, ShoppingCart> _carts = new Dictionary<string, ShoppingCart>();

        public FakeCartStorage(ShoppingCart cart = null, string cartId = null)
        {
            _carts[cartId ?? ""] = cart != null
                ? new ShoppingCart(cart.Items)
                : new ShoppingCart();
        }

        public string GetUniqueCartId(string shoppingCartId)
            => Guid.NewGuid().ToString();

        public Task<ShoppingCart> Retrieve(string shoppingCartId = null)
        {
            if (!_carts.TryGetValue(shoppingCartId ?? "", out var cart))
            {
                cart = new ShoppingCart();
                _carts.Add(shoppingCartId ?? "", cart);
            }
            return Task.FromResult(cart);
        }

        public Task Store(ShoppingCart cart, string shoppingCartId = null)
        {
            _carts[shoppingCartId ?? ""] = new ShoppingCart(cart.Items);
            return Task.CompletedTask;
        }
    }
}
