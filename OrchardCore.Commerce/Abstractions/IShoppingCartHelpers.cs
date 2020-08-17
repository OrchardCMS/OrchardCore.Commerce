using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IShoppingCartHelpers
    {
        ShoppingCartLineViewModel GetExistingLine(ShoppingCartViewModel cart, ShoppingCartLineViewModel line);
        bool IsSameProductAs(ShoppingCartLineViewModel line, ShoppingCartLineViewModel other);
        Task<ShoppingCart> ParseCart(ShoppingCartUpdateModel cart);
        Task<ShoppingCartItem> ParseCartLine(ShoppingCartLineUpdateModel line);
        HashSet<IProductAttributeValue> ParseAttributes(ShoppingCartLineUpdateModel line, ContentTypeDefinition type);
        Task<ShoppingCart> Deserialize(string serializedCart);
        Task<string> Serialize(ShoppingCart cart);
    }
}
