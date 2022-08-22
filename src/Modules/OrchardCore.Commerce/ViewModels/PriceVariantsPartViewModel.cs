using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.ContentManagement;
using System.Collections.Generic;

namespace OrchardCore.Commerce.ViewModels;

public class PriceVariantsPartViewModel
{
    public IDictionary<string, decimal?> VariantsValues { get; private set; } = new Dictionary<string, decimal?>();
    public IDictionary<string, string> VariantsCurrencies { get; private set; } = new Dictionary<string, string>();

    public IEnumerable<ICurrency> Currencies { get; set; }

    [BindNever]
    public ContentItem ContentItem { get; set; }

    [BindNever]
    public PriceVariantsPart PriceVariantsPart { get; set; }

    [BindNever]
    public IDictionary<string, Amount> Variants { get; private set; } = new Dictionary<string, Amount>();

    public void InitializeVariants(
        IDictionary<string, Amount> variants,
        IDictionary<string, decimal?> values,
        IDictionary<string, string> currencies)
    {
        Variants = variants;
        VariantsValues = values;
        VariantsCurrencies = currencies;
    }
}
