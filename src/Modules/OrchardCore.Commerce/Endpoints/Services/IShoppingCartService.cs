using OrchardCore.Commerce.ViewModels;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Endpoints;
public interface IShoppingCartService
{
    Task<string> UpdateAsync(ShoppingCartUpdateModel cart, string token, string shoppingCartId = null);
    Task<string> AddItemAsync(ShoppingCartLineUpdateModel line, string token, string shoppingCartId = null);
    Task<string> RemoveLineAsync(ShoppingCartLineUpdateModel line, string shoppingCartId = null);
}
