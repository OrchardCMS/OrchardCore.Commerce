using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.DisplayManagement;
using System.Collections.Generic;

namespace OrchardCore.Commerce.ViewModels;

public class ShoppingCartViewModel
{
    public IList<LocalizedHtmlString> Headers { get; } = new List<LocalizedHtmlString>();
    public IList<List<IShape>> Lines { get; } = new List<List<IShape>>();
    public string Id { get; set; }
    public IEnumerable<Amount> Totals { get; set; }
}
