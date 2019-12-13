using System.Collections.Generic;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.ViewModels
{
    public class PriceBooksIndexViewModel
    {
        public List<dynamic> ContentItems { get; set; }
        public PriceBookIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class PriceBookIndexOptions
    {
        public string Search { get; set; }
    }
}
