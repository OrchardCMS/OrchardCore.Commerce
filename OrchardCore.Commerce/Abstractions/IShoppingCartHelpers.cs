using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Commerce.Abstractions;

public interface IShoppingCartHelpers
{
    ShoppingCartLineViewModel GetExistingLine(ShoppingCartViewModel cart, ShoppingCartLineViewModel line);
    bool IsSameProductAs(ShoppingCartLineViewModel line, ShoppingCartLineViewModel other);
    Task<ShoppingCart> ParseCartAsync(ShoppingCartUpdateModel cart);
    Task<ShoppingCartItem> ParseCartLineAsync(ShoppingCartLineUpdateModel line);
    ISet<IProductAttributeValue> ParseAttributes(ShoppingCartLineUpdateModel line, ContentTypeDefinition type);
    Task<ShoppingCart> DeserializeAsync(string serializedCart);
    Task<string> SerializeAsync(ShoppingCart cart);
}
