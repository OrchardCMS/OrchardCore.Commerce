using System.Collections.Generic;

namespace OrchardCore.Commerce.ViewModels
{
    public class ShoppingCartLineUpdateModel
    {
        public int Quantity { get; set; }
        public string ProductSku { get; set; }
        public IDictionary<string, string[]> Attributes { get; set; }
    }
}