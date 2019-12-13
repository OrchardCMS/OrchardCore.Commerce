using System.Collections.Generic;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.ViewModels
{
    public class PriceBookProductPartDisplayViewModel
    {
        public IEnumerable<PriceBookPrice> PriceBookPrices { get; set; }
    }
}
