using OrchardCore.Commerce.ViewModels;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Endpoints;
#pragma warning disable SA1600 // Elements should be documented
public interface IShoppingCartService
#pragma warning restore SA1600 // Elements should be documented
{
    Task<string> UpdateAsync(ShoppingCartUpdateModel cart, string token, string shoppingCartId = null);
    Task<string> AddItemAsync(ShoppingCartLineUpdateModel line, string token, string shoppingCartId = null);
    Task<string> RemoveLineAsync(ShoppingCartLineUpdateModel line, string shoppingCartId = null);
}
