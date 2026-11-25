using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FieldSettings = OrchardCore.Commerce.Abstractions.IPredefinedValuesProductAttributeFieldSettings;
using Service = OrchardCore.Commerce.Abstractions.IPredefinedValuesProductAttributeService;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Service for working with <see cref="FieldSettings"/>.
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
        // Prevent edge cases by cleaning the inner collections and immediately returning the list if there is no more
        // than one sequence.
        var list = sequences
            .Select(values => values.ToList())
            .Where(sequence => sequence.Count > 0)
            .ToList();

        if (list.Count <= 1) return list;

        IEnumerable<IEnumerable<T>> emptyProduct = [[]];
        return list.Aggregate(emptyProduct, (accumulator, sequence) =>
            accumulator.SelectMany(_ => sequence, (accumulatorSequence, item) => accumulatorSequence.Concat([item])));
    }
}
