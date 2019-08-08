using System.Collections.Generic;

namespace OrchardCore.Commerce.ViewModels
{
    public class ShoppingCartViewModel
    {
        public IList<ShoppingCartLineViewModel> Lines {get;set;}
        public string Id { get; set; }
    }
}
