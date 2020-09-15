using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IShoppingCartPersistence
    {
        Task<ShoppingCart> Retrieve(string shoppingCartId = null);
        Task Store(ShoppingCart items, string shoppingCartId = null);
        string GetUniqueCartId(string shoppingCartId);
    }
}
