using OrchardCore.Commerce.Abstractions.ViewModels;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.DisplayManagement;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Endpoints.ViewModels;
public class ShoppingCartViewModelVM
{
    public ShoppingCartViewModelVM(ShoppingCartViewModel model)
    {
        Id = model.Id;
        Totals = model.Totals;
        Lines = model.Lines;
        TableShapes = model.TableShapes;
        foreach (var e in model.InvalidReasons)
        {
            InvalidReasons.Add(e.Value);
        }

        foreach (var e in model.Headers)
        {
            Headers.Add(e.Name);
        }
    }

    public string Id { get; set; }
    public IList<string> InvalidReasons { get; } = new List<string>();
    public IList<string> Headers { get; } = new List<string>();
    public IList<List<IShape>> TableShapes { get; }
    public IList<ShoppingCartLineViewModel> Lines { get; }
    public IList<Amount> Totals { get; }
}
