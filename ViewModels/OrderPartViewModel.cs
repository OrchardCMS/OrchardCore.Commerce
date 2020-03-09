using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Money;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.ViewModels
{
    public class OrderPartViewModel
    {
        public IList<OrderLineItemViewModel> LineItems { get; set; }

        public Amount Total { get; set; }

        [BindNever]
        public ContentItem ContentItem { get; set; }

        [BindNever]
        public OrderPart OrderPart { get; set; }
    }
}
