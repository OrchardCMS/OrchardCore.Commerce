using OrchardCore.Commerce.Abstractions.ViewModels;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Extensions;
using OrchardCore.Commerce.Promotion.Extensions;
using OrchardCore.Commerce.Promotion.Models;
using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Models;

public record PromotionAndTaxProviderContext(
    IEnumerable<PromotionAndTaxProviderContextLineItem> Items,
    IEnumerable<Amount> TotalsByCurrency,
    Address ShippingAddress,
    Address BillingAddress,
    DateTime? PurchaseDateTime = null,
    string VatNumber = null,
    bool Stored = false,
    bool IsCorporation = false)
{
    // This used to be a constructor, but has been turned into a factory method to avoid causing JSON serialization
    // exceptions. See https://github.com/dotnet/runtime/issues/45373#issuecomment-812091894 for more info on the
    // problem in general.
    public static PromotionAndTaxProviderContext FromShoppingCartLineViewModels(
        IEnumerable<ShoppingCartLineViewModel> lines,
        IEnumerable<Amount> totalsByCurrency,
        Address shippingAddress,
        Address billingAddress,
        DateTime? purchaseDateTime = null) =>
        new(
            lines.Select(line => new PromotionAndTaxProviderContextLineItem(
                line.Product,
                line.UnitPrice,
                line.Quantity,
                line.Product.GetAllDiscountInformation())),
            totalsByCurrency,
            shippingAddress,
            billingAddress,
            purchaseDateTime);

    public async Task<PromotionAndTaxProviderContext> UpdateAsync(
        Func<PromotionAndTaxProviderContextLineItem, DateTime?, Task<PromotionAndTaxProviderContextLineItem>> updater)
    {
        var items = Items.AsList();

        var newContextLineItems =
            await items.AwaitEachAsync(async item => await updater(item, PurchaseDateTime));

        var updatedTotals = TotalsByCurrency
            .Select(total =>
            {
                var currency = total.Currency.CurrencyIsoCode;
                return newContextLineItems
                    .Where(item => item.Subtotal.Currency.CurrencyIsoCode == currency)
                    .Select(item => item.Subtotal)
                    .Sum();
            });

        return this with { Items = newContextLineItems, TotalsByCurrency = updatedTotals };
    }
}

public record PromotionAndTaxProviderContextLineItem(IContent Content, Amount UnitPrice, int Quantity, IEnumerable<DiscountInformation> Discounts)
{
    public Amount Subtotal => UnitPrice * Quantity;
}
