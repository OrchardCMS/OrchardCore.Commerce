using OrchardCore.Commerce.Abstractions.ViewModels;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.DisplayManagement;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Endpoints.ViewModels;
public class ShoppingCartViewModelVM
{
    public ShoppingCartViewModelVM(ShoppingCartViewModel model)
    {
        this.Id = model.Id;
        this.Totals = model.Totals;
        this.Lines = model.Lines;
        this.TableShapes = model.TableShapes;
        foreach (var e in model.InvalidReasons)
        {
            this.InvalidReasons.Add(e.Value);
        }
        foreach (var e in model.Headers)
        {
            this.Headers.Add(e.Name);
        }
    }
    public string Id { get; set; }
    public IList<string> InvalidReasons { get; } = new List<string>();
    public IList<string> Headers { get; } = new List<string>();
    public IList<List<IShape>> TableShapes { get; set; } = new List<List<IShape>>();
    public IList<ShoppingCartLineViewModel> Lines { get; set; } = new List<ShoppingCartLineViewModel>();
    public IList<Amount> Totals { get; set; } = new List<Amount>();
}
