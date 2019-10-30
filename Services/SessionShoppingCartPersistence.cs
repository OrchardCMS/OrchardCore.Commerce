using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Services
{
    public class SessionShoppingCartPersistence : IShoppingCartPersistence
    {
        const string ShoppingCartPrefix = "OrchardCore:Commerce:ShoppingCart:";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IShoppingCartHelpers _shoppingCartHelpers;

        public SessionShoppingCartPersistence(
            IHttpContextAccessor httpContextAccessor,
            IShoppingCartHelpers shoppingCartHelpers)
        {
            _httpContextAccessor = httpContextAccessor;
            _shoppingCartHelpers = shoppingCartHelpers;
        }

        private ISession Session => _httpContextAccessor.HttpContext.Session;

        public async Task<IList<ShoppingCartItem>> Retrieve(string shoppingCartId = null)
        {
            var cartString = Session.GetString(ShoppingCartPrefix + (shoppingCartId ?? ""));
            return await _shoppingCartHelpers.Deserialize(cartString);
        }

        public async Task Store(IList<ShoppingCartItem> items, string shoppingCartId = null)
        {
            var cartString = await _shoppingCartHelpers.Serialize(items);
            Session.SetString(ShoppingCartPrefix + (shoppingCartId ?? ""), cartString);
        }
    }
}
