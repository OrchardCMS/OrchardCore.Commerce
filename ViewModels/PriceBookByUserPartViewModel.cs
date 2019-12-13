using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.ViewModels
{
    public class PriceBookByUserPartViewModel
    {
        public string UserName { get; set; }
        public string PriceBookContentItemId { get; set; }

        [BindNever]
        public IEnumerable<IContent> PriceBooks { get; set; }
    }
}
