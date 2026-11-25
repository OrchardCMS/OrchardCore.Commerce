using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FieldSettings = OrchardCore.Commerce.Abstractions.IPredefinedValuesProductAttributeFieldSettings;
using Service = OrchardCore.Commerce.Abstractions.IProductAttributeService;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// A service for working with <see cref="ProductAttributeField"/>s.
/// </summary>
public interface IProductAttributeService
{
    /// <summary>
    /// Returns <see cref="ProductAttributeField"/>s and their settings of a given <paramref name="product"/>.
    /// </summary>
    Task<IEnumerable<ProductAttributeDescription>> GetProductAttributeFieldsAsync(ContentItem product);

    /// <summary>
    /// Returns the part and field definitions of the given <paramref name="type"/>'s specified <paramref name="attributeName"/>.
    /// </summary>
    (ContentTypePartDefinition PartDefinition, ContentPartFieldDefinition FieldDefinition)
        GetFieldDefinition(ContentTypeDefinition type, string attributeName);

    /// <summary>
    /// Updates the <see cref="ProductPart.CanBeBought"/> dictionary and applies the changes to the SKU key of <see cref="ProductAttributeField"/>.
    /// </summary>
    /// <param name="part"><see cref="ProductPart"/>.</param>
    void UpdateCanBeBoughtForProductAttributeField(ProductPart part, string skuBefore);

    /// <summary>
    /// Filters <see cref="IProductAttributeService.GetProductAttributeFieldsAsync"/> result to only those attribute fields
    /// that have predefined values.
    /// </summary>
    Task<IEnumerable<ProductAttributeDescription>> GetProductAttributesRestrictedToPredefinedValuesAsync(ContentItem product);

    /// <summary>
    /// returns the SKU suffix used to distinguish between product variants.
    /// </summary>
    Task<string> GetProductKey(ContentItem product, ICollection<IProductAttributeValue> attributes, IShoppingCartSerializer serializer);
}

public static class ProductAttributeServiceExtensions
{
    public static async Task<IEnumerable<IEnumerable<object>>> GetProductAttributesPredefinedValuesAsync(
        this Service service, ContentItem product) =>
        (await service.GetProductAttributesRestrictedToPredefinedValuesAsync(product))
        .Where(description => description.Settings is FieldSettings)
        .Select(description => ((FieldSettings)description.Settings).PredefinedValues.ToList())
        .ToList();

    public static async Task<IEnumerable<string>> GetProductAttributesCombinationsAsync(
        this Service service, ContentItem product) =>
        (await service.GetProductAttributesPredefinedValuesAsync(product))
        .CartesianProduct()
        .Select(predefinedValues => string.Join('-', predefinedValues));

    private static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
    {
        // Prevent edge cases by cleaning the inner collections and handling when "sequences" only has 0 - 1 items.
        var list = sequences
            .Select(values => values.ToList())
            .Where(sequence => sequence.Count > 0)
            .ToList();

        if (list.Count == 0) return list;
        if (list.Count == 1) return list[0].Select(item => new[] { item });

        IEnumerable<IEnumerable<T>> emptyProduct = [[]];
        return list.Aggregate(emptyProduct, (accumulator, sequence) =>
            accumulator.SelectMany(_ => sequence, (accumulatorSequence, item) => accumulatorSequence.Concat([item])));
    }
}
