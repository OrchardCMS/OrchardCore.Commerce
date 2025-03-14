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
    /// Returns <see langword="true"/> if all of the model <see cref="PromotionAndTaxProviderContext.Items"/> are
    /// applicable, <see langword="false"/> if none are, and throws <see cref="InvalidOperationException"/> if only some
    /// are valid but not all, because that suggests an invalid state.
    /// </summary>
    protected static async Task<bool> AllOrNoneAsync(
        PromotionAndTaxProviderContext model,
        Func<IList<PromotionAndTaxProviderContextLineItem>,
            Task<int>> getCountAsync)
    {
        var items = model.Items.AsList();
        var count = await getCountAsync(items);

        if (count == 0) return false;
        if (count == items.Count) return true;
        throw new InvalidOperationException("Some, but not all products have gross price. This is invalid.");
    }

    /// <inheritdoc cref="AllOrNoneAsync(PromotionAndTaxProviderContext, Func{IList{PromotionAndTaxProviderContextLineItem}, Task{int}})"/>
    protected static Task<bool> AllOrNoneAsync(
        PromotionAndTaxProviderContext model,
        Func<IList<PromotionAndTaxProviderContextLineItem>, int> getCount) =>
        AllOrNoneAsync(model, item => Task.FromResult(getCount(item)));
}
