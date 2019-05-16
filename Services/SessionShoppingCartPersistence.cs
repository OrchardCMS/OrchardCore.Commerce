using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Services
{
    public class SessionShoppingCartPersistence : IShoppingCartPersistence
    {
        const string ShoppingCartPrefix = "OrchardCore:Commerce:ShoppingCart:";

        IHttpContextAccessor _httpContextAccessor;

        public SessionShoppingCartPersistence(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ISession Session => _httpContextAccessor.HttpContext.Session;

        public Task<IList<ShoppingCartItem>> Retrieve(string shoppingCartId = null)
        {
            var cartString = Session.GetString(ShoppingCartPrefix + (shoppingCartId ?? ""));
            if (String.IsNullOrEmpty(cartString))
            {
                return Task.FromResult((IList<ShoppingCartItem>)new List<ShoppingCartItem>());
            }
            var cart = JsonConvert.DeserializeObject<List<ShoppingCartItem>>(cartString);
            return Task.FromResult((IList<ShoppingCartItem>)cart);
        }

        public Task Store(IList<ShoppingCartItem> items, string shoppingCartId = null)
        {
            var cartString = JsonConvert.SerializeObject(items);
            Session.SetString(ShoppingCartPrefix + (shoppingCartId ?? ""), cartString);
            return Task.CompletedTask;
        }
    }
}
