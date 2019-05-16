using System.Collections.Generic;
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

        [HttpPost]
        public async Task<IList<ShoppingCartItem>> Index(string shoppingCartId)
            => await _shoppingCartPersistence.Retrieve(shoppingCartId);
    }
}
