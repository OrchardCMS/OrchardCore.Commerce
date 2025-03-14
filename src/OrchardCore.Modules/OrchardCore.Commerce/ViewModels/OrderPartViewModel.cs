using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.ContentManagement;
using System.Collections.Generic;

namespace OrchardCore.Commerce.ViewModels;

public class OrderPartViewModel
{
    public IList<OrderLineItemViewModel> LineItems { get; } = [];

    public Amount Total { get; set; }
    public IList<Abstractions.Models.Payment> Charges { get; } = [];

    [BindNever]
    public ContentItem ContentItem { get; set; }

    [BindNever]
    public OrderPart OrderPart { get; set; }
}
