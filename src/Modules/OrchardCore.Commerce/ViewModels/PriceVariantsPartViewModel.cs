using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;

namespace OrchardCore.Commerce.ViewModels;

public class PriceVariantsPartViewModel
{
    public IDictionary<string, decimal?> VariantsValues { get; } = new Dictionary<string, decimal?>(StringComparer.OrdinalIgnoreCase);
    public IDictionary<string, string> VariantsCurrencies { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    [BindNever]
    public IEnumerable<ICurrency> Currencies { get; set; }

    [BindNever]
    public ContentItem ContentItem { get; set; }

    [BindNever]
    public PriceVariantsPart PriceVariantsPart { get; set; }

    [BindNever]
    public IDictionary<string, Amount> Variants { get; } = new Dictionary<string, Amount>(StringComparer.OrdinalIgnoreCase);

    public void InitializeVariants(
        IDictionary<string, Amount> variants,
        IDictionary<string, decimal?> values,
        IDictionary<string, string> currencies)
    {
        Variants.SetItems(variants);
        VariantsValues.SetItems(values);
        VariantsCurrencies.SetItems(currencies);
    }
}
