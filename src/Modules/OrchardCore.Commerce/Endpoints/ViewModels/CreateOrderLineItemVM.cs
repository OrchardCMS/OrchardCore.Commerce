using OrchardCore.Commerce.Abstractions.Models;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Endpoints.ViewModels;
public class CreateOrderLineItemVM
{
#pragma warning disable CA2227 // 集合属性应为只读
    public IList<OrderLineItem> LineItems { get; set; }
#pragma warning restore CA2227 // 集合属性应为只读
    public OrderPart OrderPart { get; set; }
}
