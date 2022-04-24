using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Service for working with <see cref="IPredefinedValuesProductAttributeFieldSettings"/>.
/// </summary>
public interface IPredefinedValuesProductAttributeService
{
    /// <summary>
    /// Filters <see cref="IProductAttributeService.GetProductAttributeFields"/> result to only those attribute fields
    /// that have predefined values.
    /// </summary>
    IEnumerable<ProductAttributeDescription> GetProductAttributesRestrictedToPredefinedValues(ContentItem product);
}

public static class PredefinedValuesProductAttributeServiceExtensions
{
    public static IEnumerable<IEnumerable<object>> GetProductAttributesPredefinedValues(
        this IPredefinedValuesProductAttributeService service,
        ContentItem product) =>
        service.GetProductAttributesRestrictedToPredefinedValues(product)
            .Select(x => (x.Settings as IPredefinedValuesProductAttributeFieldSettings)?.PredefinedValues.ToList())
            .ToList();

    public static IEnumerable<string> GetProductAttributesCombinations(
        this IPredefinedValuesProductAttributeService service,
        ContentItem product) =>
        CartesianProduct(service.GetProductAttributesPredefinedValues(product))
            .Select(x => string.Join("-", x));

    private static IEnumerable<IEnumerable<T>> CartesianProduct<T>(IEnumerable<IEnumerable<T>> sequences)
    {
        IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
        return sequences.Aggregate(
            emptyProduct,
            (accumulator, sequence) =>
                accumulator.SelectMany(_ => sequence, (accumulatorSequence, item) => accumulatorSequence.Concat(new[] { item })));
    }
}
