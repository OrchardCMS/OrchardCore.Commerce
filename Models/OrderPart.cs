using System.Collections.Generic;
using Money;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Models
{
    public class OrderPart : ContentPart
    {
        public IList<OrderLineItem> LineItems { get; set; }
        public Amount Total { get; set; }
    }
}
