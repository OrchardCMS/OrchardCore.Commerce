using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Models;

public record PromotionAndTaxProviderContext(
    IEnumerable<PromotionAndTaxProviderContextLineItem> Items,
    IEnumerable<Amount> TotalsByCurrency)
{
    public PromotionAndTaxProviderContext(
        ICollection<ShoppingCartLineViewModel> lines,
        IEnumerable<Amount> totalsByCurrency)
        : this(
            lines.Select(line => new PromotionAndTaxProviderContextLineItem(line.Product, line.UnitPrice, line.Quantity)),
            totalsByCurrency)
    {
    }
}

public record PromotionAndTaxProviderContextLineItem(IContent Content, Amount UnitPrice, int Quantity)
{
    public Amount Subtotal => UnitPrice * Quantity;
}
