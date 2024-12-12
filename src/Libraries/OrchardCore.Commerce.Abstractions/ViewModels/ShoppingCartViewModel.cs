using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.DisplayManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace OrchardCore.Commerce.Abstractions.ViewModels;

public class ShoppingCartViewModel
{
    public string Id { get; set; }

    [JsonIgnore]
    public IList<LocalizedHtmlString> InvalidReasons { get; } = [];

    [JsonIgnore]
    public IList<LocalizedHtmlString> Headers { get; } = [];

    [JsonIgnore]
    public IList<List<IShape>> TableShapes { get; } = [];
    public IList<ShoppingCartLineViewModel> Lines { get; } = [];
    public IList<Amount> Totals { get; } = [];

    public IList<Amount> GetTotalsOrThrowIfEmpty() =>
        Totals.Any()
            ? Totals
            : throw new InvalidOperationException("Cannot create a payment without shopping cart total(s)!");
}
