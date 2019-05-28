using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Helpers;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IShoppingCartPersistence _shoppingCartPersistence;
        public ShoppingCartController(IShoppingCartPersistence shoppingCartPersistence)
        {
            _shoppingCartPersistence = shoppingCartPersistence;
        }

        [HttpGet]
        public async Task<IList<ShoppingCartItem>> Index(string shoppingCartId = null)
            => await _shoppingCartPersistence.Retrieve(shoppingCartId);

        [HttpPost]
        public async Task<IList<ShoppingCartItem>> AddItem(ShoppingCartItem item, string shoppingCartId = null)
        {
            var cart = await _shoppingCartPersistence.Retrieve(shoppingCartId);
            var existingItem = cart.GetExistingItem(item);
            if (existingItem != null)
            {
                var index = cart.RemoveItem(existingItem);
                cart.Insert(index, new ShoppingCartItem(existingItem.Quantity + item.Quantity, item.ProductSku, item.Attributes));
            }
            else
            {
                cart.Add(item);
            }
            await _shoppingCartPersistence.Store(cart, shoppingCartId);
            return cart;
        }

        [HttpPost]
        public async Task<IList<ShoppingCartItem>> RemoveItem(ShoppingCartItem item, string shoppingCartId = null)
        {
            var cart = await _shoppingCartPersistence.Retrieve(shoppingCartId);
            cart.RemoveItem(item);
            await _shoppingCartPersistence.Store(cart, shoppingCartId);
            return cart;
        }
    }
}
