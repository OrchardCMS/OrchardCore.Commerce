using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.DisplayManagement;
using System.Collections.Generic;

namespace OrchardCore.Commerce.ViewModels;

public class ShoppingCartViewModel
{
    public string Id { get; set; }
    public bool IsInvalid { get; set; }
    public IList<LocalizedHtmlString> Headers { get; } = new List<LocalizedHtmlString>();
    public IList<List<IShape>> TableShapes { get; } = new List<List<IShape>>();
    public IList<ShoppingCartLineViewModel> Lines { get; } = new List<ShoppingCartLineViewModel>();
    public IList<Amount> Totals { get; } = new List<Amount>();
}
