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
    /// Invoked when the line items of the shopping cart are displayed.
    /// </summary>
    /// <param name="headers">The column headers used in the display table.</param>
    /// <param name="lines">The line items used to render the table.</param>
    Task LinesDisplayingAsync(IList<LocalizedHtmlString> headers, ShoppingCartLineViewModel[] lines) =>
        Task.CompletedTask;

    /// <summary>
    /// Invoked when the total cost of the shopping cart are displayed.
    /// </summary>
    /// <param name="totals">The totals </param>
    /// <param name="lines">The line items.</param>
    /// <returns></returns>
    Task TotalsDisplayingAsync(IList<Amount> totals, ShoppingCartLineViewModel[] lines);
}
