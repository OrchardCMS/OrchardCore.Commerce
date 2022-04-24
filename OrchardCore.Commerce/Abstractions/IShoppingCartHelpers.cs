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

    /// <summary>
    /// Returns a JSON serialized <see langword="string"/> of <paramref name="cart"/>.
    /// </summary>
    Task<string> SerializeAsync(ShoppingCart cart);

    /// <summary>
    /// Returns a deserialized object from JSON string <paramref name="serializedCart"/>.
    /// </summary>
    Task<ShoppingCart> DeserializeAsync(string serializedCart);

    /// <summary>
    /// Validates and updates the <paramref name="parsedLine"/>. If fails sends notification and returns <see
    /// langword="null"/>.
    /// </summary>
    Task<ShoppingCartItem> ValidateParsedCartLineAsync(
        ShoppingCartLineUpdateModel line,
        ShoppingCartItem parsedLine);
}
