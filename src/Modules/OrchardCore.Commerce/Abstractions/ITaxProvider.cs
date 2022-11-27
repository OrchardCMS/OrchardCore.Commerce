using OrchardCore.Commerce.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Updates prices with tax.
/// </summary>
public interface ITaxProvider : ISortableUpdaterProvider<PromotionAndTaxProviderContext>
{
    /// <summary>
    /// Returns <see langword="true"/> if all of the model <see cref="TaxProviderContext.Items"/> are applicable, <see langword="false"/> if none are,
    /// and throws <see cref="InvalidOperationException"/> if only some are valid but not all, because that suggests an invalid state.
    /// </summary>
    protected static async Task<bool> AllOrNoneAsync(TaxProviderContext model, Func<IList<TaxProviderContextLineItem>, Task<int>> getCountAsync)
    {
        var items = model.Items.AsList();
        var count = await getCountAsync(items);

        if (count == 0) return false;
        if (count == items.Count) return true;
        throw new InvalidOperationException("Some, but not all products have gross price. This is invalid.");
    }

    /// <inheritdoc cref="AllOrNoneAsync(TaxProviderContext, Func{IList{OrchardCore.Commerce.Models.TaxProviderContextLineItem}, Task{int}})"/>
    protected static Task<bool> AllOrNoneAsync(TaxProviderContext model, Func<IList<TaxProviderContextLineItem>, int> getCount) =>
        AllOrNoneAsync(model, item => Task.FromResult(getCount(item)));
}
