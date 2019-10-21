using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IShoppingCartHelpers
    {
        ShoppingCartItem GetExistingItem(IList<ShoppingCartItem> cart, ShoppingCartItem item);
        ShoppingCartLineViewModel GetExistingLine(ShoppingCartViewModel cart, ShoppingCartLineViewModel line);
        int IndexOfProduct(IList<ShoppingCartItem> cart, ShoppingCartItem item);
        bool IsSameProductAs(ShoppingCartItem item, ShoppingCartItem other);
        bool IsSameProductAs(ShoppingCartLineViewModel line, ShoppingCartLineViewModel other);
        int RemoveItem(IList<ShoppingCartItem> cart, ShoppingCartItem item);
        Task<IList<ShoppingCartItem>> ParseCart(ShoppingCartUpdateModel cart);
        Task<ShoppingCartItem> ParseCartLine(ShoppingCartLineUpdateModel line);
        HashSet<IProductAttributeValue> ParseAttributes(ShoppingCartLineUpdateModel line, ContentTypeDefinition type);
        Task<IList<ShoppingCartItem>> Deserialize(string serializedCart);
        Task<string> Serialize(IList<ShoppingCartItem> cart);
    }
}