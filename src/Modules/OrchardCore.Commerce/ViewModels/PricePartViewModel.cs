using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.ContentManagement;
using System.Collections.Generic;

namespace OrchardCore.Commerce.ViewModels;

public class PricePartViewModel
{
    public decimal PriceValue { get; set; }
    public string PriceCurrency { get; set; }

    public IEnumerable<ICurrency> Currencies { get; set; }

    public ICurrency CurrentDisplayCurrency { get; set; }

    [BindNever]
    public ContentItem ContentItem { get; set; }

    [BindNever]
    public PricePart PricePart { get; set; }

    [BindNever]
    public Amount Price { get; set; }
}
