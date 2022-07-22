using Microsoft.AspNetCore.Mvc.ModelBinding;
using Money;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using System.Collections.Generic;

namespace OrchardCore.Commerce.ViewModels;

public class OrderPartViewModel
{
    public IList<OrderLineItemViewModel> LineItems { get; } = new List<OrderLineItemViewModel>();

    public Amount Total { get; set; }
    public IList<IPayment> Charges { get; } = new List<IPayment>();

    [BindNever]
    public ContentItem ContentItem { get; set; }

    [BindNever]
    public OrderPart OrderPart { get; set; }
}
