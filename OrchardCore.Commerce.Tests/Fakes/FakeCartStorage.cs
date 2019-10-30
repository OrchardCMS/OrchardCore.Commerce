using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Tests.Fakes
{
    public class FakeCartStorage : IShoppingCartPersistence
    {
        private Dictionary<string, IList<ShoppingCartItem>> _carts = new Dictionary<string, IList<ShoppingCartItem>>();

        public FakeCartStorage(IList<ShoppingCartItem> items = null, string cartId = null)
        {
            _carts[cartId ?? ""] = items != null ? new List<ShoppingCartItem>(items) : new List<ShoppingCartItem>();
        }

        public Task<IList<ShoppingCartItem>> Retrieve(string shoppingCartId = null)
        {
            if (!_carts.TryGetValue(shoppingCartId ?? "", out var cart))
            {
                cart = new List<ShoppingCartItem>();
                _carts.Add(shoppingCartId ?? "", cart);
            }
            return Task.FromResult(cart);
        }

        public Task Store(IList<ShoppingCartItem> items, string shoppingCartId = null)
        {
            _carts[shoppingCartId ?? ""] = new List<ShoppingCartItem>(items);
            return Task.CompletedTask;
        }
    }
}
