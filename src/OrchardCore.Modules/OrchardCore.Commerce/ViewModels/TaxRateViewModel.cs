using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using System.Linq;

namespace OrchardCore.Commerce.ViewModels;

public class TaxRateViewModel
{
    public PromotionAndTaxProviderContext Context { get; set; }

    public Amount? GrossPrice => Context?.Items.SingleOrDefault()?.UnitPrice is { IsValid: true } price ? price : null;
}
