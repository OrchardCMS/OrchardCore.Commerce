using OrchardCore.Commerce.Tax.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.MoneyDataType;

public static class AmountExtensions
{
    public static Amount WithTax(this Amount netAmount, decimal taxRate) =>
        new(netAmount.Value * ToMultiplier(taxRate), netAmount.Currency);

    public static Amount WithTax(this Amount netAmount, IContent contentWithTaxPart) =>
        WithTax(netAmount, contentWithTaxPart.As<TaxPart>()?.TaxRate.Value ?? 0);

    public static Amount WithoutTax(this Amount grossAmount, decimal taxRate) =>
        new(grossAmount.Value / ToMultiplier(taxRate), grossAmount.Currency);

    private static decimal ToMultiplier(decimal taxRate) => 1 + (taxRate / 100m);
}
