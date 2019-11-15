using System.Collections.Generic;
using Money;
using OrchardCore.Commerce.Abstractions;

namespace OrchardCore.Commerce.ViewModels
{
    public class ShoppingCartLineViewModel
    {
        public int Quantity { get; set; }
        public string ProductSku { get; set; }
        public string ProductName { get; set; }
        public string ProductUrl { get; set; }
        public string ProductImageUrl { get; set; }
        public Amount UnitPrice { get; set; }
        public Amount LinePrice { get; set; }
        public IDictionary<string, IProductAttributeValue> Attributes { get; set; }
    }
}