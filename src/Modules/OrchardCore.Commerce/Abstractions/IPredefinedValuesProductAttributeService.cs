using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Service for working with <see cref="IPredefinedValuesProductAttributeFieldSettings"/>.
/// </summary>
public interface IPredefinedValuesProductAttributeService
{
    /// <summary>
    /// Filters <see cref="IProductAttributeService.GetProductAttributeFieldsAsync"/> result to only those attribute fields
    /// that have predefined values.
    /// </summary>
    Task<IEnumerable<ProductAttributeDescription>> GetProductAttributesRestrictedToPredefinedValuesAsync(ContentItem product);
}

public static class PredefinedValuesProductAttributeServiceExtensions
{
    public static async Task<IEnumerable<IEnumerable<object>>> GetProductAttributesPredefinedValuesAsync(
        this IPredefinedValuesProductAttributeService service,
        ContentItem product) =>
        (await service.GetProductAttributesRestrictedToPredefinedValuesAsync(product))
            .Where(description => description.Settings is IPredefinedValuesProductAttributeFieldSettings)
            .Select(description =>
                ((IPredefinedValuesProductAttributeFieldSettings)description.Settings).PredefinedValues.ToList())
            .ToList();

    public static async Task<IEnumerable<string>> GetProductAttributesCombinationsAsync(
        this IPredefinedValuesProductAttributeService service,
        ContentItem product) =>
        CartesianProduct(await service.GetProductAttributesPredefinedValuesAsync(product))
            .Select(predefinedValues => string.Join('-', predefinedValues));

    private static IEnumerable<IEnumerable<T>> CartesianProduct<T>(IEnumerable<IEnumerable<T>> sequences)
    {
        IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
        return sequences.Aggregate(
            emptyProduct,
            (accumulator, sequence) =>
                accumulator.SelectMany(
                    _ => sequence,
                    (accumulatorSequence, item) => accumulatorSequence.Concat(new[] { item })));
    }
}
