using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Models;

public record TaxProviderContext(
    IEnumerable<IContent> Contents,
    IEnumerable<Amount> Subtotals,
    IEnumerable<Amount> TotalsByCurrency)
{
    public TaxProviderContext(
        ICollection<ShoppingCartLineViewModel> lines,
        IEnumerable<Amount> totalsByCurrency)
        : this(
            lines.Select(line => line.Product),
            lines.Select(line => line.LinePrice),
            totalsByCurrency)
    {
    }
}
