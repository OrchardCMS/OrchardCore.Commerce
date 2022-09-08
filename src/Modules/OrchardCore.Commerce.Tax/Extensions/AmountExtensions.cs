using OrchardCore.Commerce.Tax.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.MoneyDataType;

public static class AmountExtensions
{
    public static Amount WithTax(this Amount amount, decimal taxRate)
    {
        var multiplier = 1 + (taxRate / 100m);
        return new Amount(amount.Value * multiplier, amount.Currency);
    }

    public static Amount WithTax(this Amount amount, IContent contentWithTaxPart) =>
        WithTax(amount, contentWithTaxPart.As<TaxPart>().TaxRate.Value!.Value);
}
