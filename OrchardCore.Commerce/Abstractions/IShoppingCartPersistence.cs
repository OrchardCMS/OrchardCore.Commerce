using System.Threading.Tasks;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Abstractions;

public interface IShoppingCartPersistence
{
    Task<ShoppingCart> RetrieveAsync(string shoppingCartId = null);
    Task StoreAsync(ShoppingCart items, string shoppingCartId = null);
    string GetUniqueCartId(string shoppingCartId);
}
