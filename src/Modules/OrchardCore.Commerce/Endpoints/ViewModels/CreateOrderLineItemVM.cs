using OrchardCore.Commerce.Abstractions.Models;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Endpoints.ViewModels;
public class CreateOrderLineItemVM
{
    public IList<OrderLineItem> LineItems { get; set; }
    public OrderPart OrderPart { get; set; }
}
