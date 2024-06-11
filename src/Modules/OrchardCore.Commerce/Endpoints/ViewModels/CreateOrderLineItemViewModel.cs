using OrchardCore.Commerce.Abstractions.Models;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Endpoints.ViewModels;
public class CreateOrderLineItemViewModel
{
    public IList<OrderLineItem> LineItems { get; init; }
    public OrderPart OrderPart { get; set; }
}
