using OrchardCore.Commerce.AddressDataType;
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
    Address ShippingAddress,
    Address BillingAddress,
    DateTime? PurchaseDateTime = null)
{
    public PromotionAndTaxProviderContext(
        IEnumerable<ShoppingCartLineViewModel> lines,
        IEnumerable<Amount> totalsByCurrency,
        Address shippingAddress,
        Address billingAddress,
        DateTime? purchaseDateTime = null)
        : this(
            lines.Select(line => new PromotionAndTaxProviderContextLineItem(line.Product, line.UnitPrice, line.Quantity)),
            totalsByCurrency,
            shippingAddress,
            billingAddress,
            purchaseDateTime)
    {
    }

    public static PromotionAndTaxProviderContext SingleProduct(IContent product, Amount netUnitPrice, Address shipping, Address billing) =>
        new(
            new[] { new PromotionAndTaxProviderContextLineItem(product, netUnitPrice, 1) },
            new[] { netUnitPrice },
            shipping,
            billing);
}

public record PromotionAndTaxProviderContextLineItem(IContent Content, Amount UnitPrice, int Quantity)
{
    public Amount Subtotal => UnitPrice * Quantity;
}
