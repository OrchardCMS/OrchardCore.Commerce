using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Controllers;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Events related to <see cref="ShoppingCartController"/>.
/// </summary>
public interface IShoppingCartEvents
{
    /// <summary>
    /// Invoked after the shopping card data is prepared, but before the shapes are rendered.
    /// </summary>
    /// <param name="totals">The line items grouped by currency and summed up.</param>
    /// <param name="headers">The column headers of the line items table.</param>
    /// <param name="lines">The line items.</param>
    /// <returns>The new versions of the matching parameters.</returns>
    Task<(IList<Amount> Totals, IList<LocalizedHtmlString> Headers)> DisplayingAsync(
        IList<Amount> totals,
        IList<LocalizedHtmlString> headers,
        ShoppingCartLineViewModel[] lines);
}
