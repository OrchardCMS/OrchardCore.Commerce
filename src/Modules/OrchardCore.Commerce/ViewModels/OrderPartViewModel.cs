using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.ViewModels;

public class OrderPartViewModel
{
    public IList<OrderLineItemViewModel> LineItems { get; } = new List<OrderLineItemViewModel>();

    public Amount Total { get; set; }
    public IList<Abstractions.Models.Payment> Charges { get; } = new List<Abstractions.Models.Payment>();

    [BindNever]
    public ContentItem ContentItem { get; set; }

    [BindNever]
    public OrderPart OrderPart { get; set; }
}
