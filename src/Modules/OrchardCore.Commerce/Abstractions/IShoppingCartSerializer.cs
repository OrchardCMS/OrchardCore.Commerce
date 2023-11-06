using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Abstractions.ProductAttributeValues;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Service for shopping cart serialization and deserialization.
/// </summary>
public interface IShoppingCartSerializer
{
    /// <summary>
    /// Returns a JSON serialized <see langword="string"/> of <paramref name="cart"/>.
    /// </summary>
    Task<string> SerializeAsync(ShoppingCart cart);

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
    /// Returns a deserialized object from JSON string <paramref name="serializedCart"/>.
    /// </summary>
    Task<ShoppingCart> DeserializeAsync(string serializedCart);

    /// <summary>
    /// Process the products attributes for a cart line item, substitute temporary storage attributes such as <see
    /// cref="RawProductAttributeValue"/>.
    /// </summary>
    ISet<IProductAttributeValue> PostProcessAttributes(IEnumerable<IProductAttributeValue> attributes, ProductPart productPart);
}
