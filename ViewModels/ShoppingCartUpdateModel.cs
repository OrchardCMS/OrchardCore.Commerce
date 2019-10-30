using System.Collections.Generic;

namespace OrchardCore.Commerce.ViewModels
{
    public class ShoppingCartUpdateModel
    {
        public IList<ShoppingCartLineUpdateModel> Lines {get;set;}
    }
}
