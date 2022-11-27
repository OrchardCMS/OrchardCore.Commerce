using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Models;

public record PromotionAndTaxProviderContext(
    IEnumerable<PromotionAndTaxProviderContextLineItem> Items,
    IEnumerable<Amount> TotalsByCurrency,
    DateTime? PurchaseDateTime = null)
{
    public PromotionAndTaxProviderContext(
        ICollection<ShoppingCartLineViewModel> lines,
        IEnumerable<Amount> totalsByCurrency,
        DateTime? purchaseDateTime = null)
        : this(
            lines.Select(line => new PromotionAndTaxProviderContextLineItem(line.Product, line.UnitPrice, line.Quantity)),
            totalsByCurrency,
            purchaseDateTime)
    {
    }
}

public record PromotionAndTaxProviderContextLineItem(IContent Content, Amount UnitPrice, int Quantity)
{
    public Amount Subtotal => UnitPrice * Quantity;
}
