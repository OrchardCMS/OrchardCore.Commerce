using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Models;

public record TaxProviderContext(
    IEnumerable<TaxProviderContextLineItem> Items,
    IEnumerable<Amount> TotalsByCurrency)
{
    public TaxProviderContext(
        ICollection<ShoppingCartLineViewModel> lines,
        IEnumerable<Amount> totalsByCurrency)
        : this(
            lines.Select(line => new TaxProviderContextLineItem(line.Product, line.UnitPrice, line.Quantity)),
            totalsByCurrency)
    {
    }

    public TaxProviderContext(ICollection<TaxProviderContextLineItem> items)
        : this(items, items.Select(item => item.Subtotal))
    {
    }
}

public record TaxProviderContextLineItem(IContent Content, Amount UnitPrice, int Quantity)
{
    public Amount Subtotal => UnitPrice * Quantity;
}
