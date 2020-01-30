using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.Commerce.Handlers
{
    public class PriceVariantsPartHandler : ContentPartHandler<PriceVariantsPart>
    {
        private IMoneyService _moneyService;

        public PriceVariantsPartHandler(IMoneyService moneyService)
        {
            _moneyService = moneyService;
        }

        public override Task LoadingAsync(LoadContentContext context, PriceVariantsPart part)
        {
            if (part.Variants != null)
            {
                foreach (var variantKey in part.Variants.Keys)
                {
                    part.Variants[variantKey] = _moneyService.EnsureCurrency(part.Variants[variantKey]);
                }
            }
            return base.LoadingAsync(context, part);
        }
    }
}
