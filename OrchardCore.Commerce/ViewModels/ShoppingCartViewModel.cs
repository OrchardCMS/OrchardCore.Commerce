using System.Collections.Generic;
using Money;

namespace OrchardCore.Commerce.ViewModels
{
    public class ShoppingCartViewModel
    {
        public IList<ShoppingCartLineViewModel> Lines { get; set; }
        public string Id { get; set; }
        public IEnumerable<Amount> Totals { get; set; }
    }
}
