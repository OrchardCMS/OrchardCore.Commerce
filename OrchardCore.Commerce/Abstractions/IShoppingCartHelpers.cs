using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// A service to work with shopping cart data.
/// </summary>
public interface IShoppingCartHelpers
{
    /// <summary>
    /// Returns the first line in <paramref name="cart"/> that has the product that <paramref name="line"/> refers to.
    /// </summary>
    ShoppingCartLineViewModel GetExistingLine(ShoppingCartViewModel cart, ShoppingCartLineViewModel line);

    /// <summary>
    /// Builds the <see cref="ShoppingCart"/> from the provided update model.
    /// </summary>
    Task<ShoppingCart> ParseCartAsync(ShoppingCartUpdateModel cart);

    /// <summary>
    /// Builds a single <see cref="ShoppingCartItem"/> from the provided update model.
    /// </summary>
    Task<ShoppingCartItem> ParseCartLineAsync(ShoppingCartLineUpdateModel line);

    /// <summary>
    /// Returns a set of attribute values using the available <see cref="IProductAttributeProvider"/>s.
    /// </summary>
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
