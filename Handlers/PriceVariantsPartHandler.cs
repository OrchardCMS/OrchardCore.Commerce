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
            part.BasePrice = _moneyService.EnsureCurrency(part.BasePrice);
            return base.LoadingAsync(context, part);
        }
    }
}
