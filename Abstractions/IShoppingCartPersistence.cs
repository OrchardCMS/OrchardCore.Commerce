using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IShoppingCartPersistence
    {
        Task<IList<ShoppingCartItem>> Retrieve(string shoppingCartId = null);
        Task Store(IList<ShoppingCartItem> items, string shoppingCartId = null);
    }
}
