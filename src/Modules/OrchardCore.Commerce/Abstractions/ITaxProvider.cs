#nullable enable

using OrchardCore.Commerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
        Func<IList<PromotionAndTaxProviderContextLineItem>, Task<int>> getCountAsync)
    {
        var items = model.Items.Where(item => item.Subtotal.Value > 0).AsList();
        var count = await getCountAsync(items);

        if (count == 0) return false;
        if (count == items.Count) return true;
        throw new InvalidOperationException(
            "Some, but not all products have gross price. This is invalid. If you want to declare a tax-free " +
            "product, make sure to still include a Tax Part for the content type. Then set the Tax Rate field to 0 " +
            "in the products.");
    }

    /// <inheritdoc cref="AllOrNoneAsync(PromotionAndTaxProviderContext, Func{IList{PromotionAndTaxProviderContextLineItem}, Task{int}})"/>
    [Obsolete("Use the overload with async callback.")]
    protected static Task<bool> AllOrNoneAsync(
        PromotionAndTaxProviderContext model,
        Func<IList<PromotionAndTaxProviderContextLineItem>, int> getCount) =>
        AllOrNoneAsync(model, item => Task.FromResult(getCount(item)));
}
