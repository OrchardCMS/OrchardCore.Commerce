using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Commerce.Abstractions;
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
            var existingItem = cart.FirstOrDefault(i => i.ProductSku == item.ProductSku
                && ((item.Attributes is null && i.Attributes is null)
                || (i.Attributes is object && i.Attributes.SetEquals(item.Attributes))));
            if (existingItem != null)
            {
                var index = cart.IndexOf(existingItem);
                cart.RemoveAt(index);
                cart.Insert(index, new ShoppingCartItem(existingItem.Quantity + item.Quantity, item.ProductSku, item.Attributes));
            }
            else
            {
                cart.Add(item);
            }
            await _shoppingCartPersistence.Store(cart, shoppingCartId);
            return cart;
        }
    }
}
