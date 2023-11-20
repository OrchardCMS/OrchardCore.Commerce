using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.DisplayManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Abstractions.ViewModels;

public class ShoppingCartViewModel
{
    public string Id { get; set; }
    public IList<LocalizedHtmlString> InvalidReasons { get; } = new List<LocalizedHtmlString>();
    public IList<LocalizedHtmlString> Headers { get; } = new List<LocalizedHtmlString>();
    public IList<List<IShape>> TableShapes { get; } = new List<List<IShape>>();
    public IList<ShoppingCartLineViewModel> Lines { get; } = new List<ShoppingCartLineViewModel>();
    public IList<Amount> Totals { get; } = new List<Amount>();

    public IList<Amount> GetTotalsOrThrowIfEmpty() =>
        Totals.Any()
            ? Totals
            : throw new InvalidOperationException("Cannot create a payment without shopping cart total(s)!");
}
