using System.Collections.Generic;
using Money;

namespace OrchardCore.Commerce.ViewModels;

public class ShoppingCartViewModel
{
    public IList<ShoppingCartLineViewModel> Lines { get; } = new List<ShoppingCartLineViewModel>();
    public string Id { get; set; }
    public IEnumerable<Amount> Totals { get; set; }

    public ShoppingCartViewModel(IEnumerable<ShoppingCartLineViewModel> lines)
    {
        if (lines != null)
        {
            foreach (var line in lines)
            {
                Lines.Add(line);
            }
        }
    }
}
