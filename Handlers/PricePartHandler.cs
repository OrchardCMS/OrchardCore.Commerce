using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.Commerce.Handlers
{
    public class PricePartHandler : ContentPartHandler<PricePart>
    {
        private IMoneyService _moneyService;

        public PricePartHandler(IMoneyService moneyService)
        {
            _moneyService = moneyService;
        }

        public override Task LoadingAsync(LoadContentContext context, PricePart part)
        {
            part.Price = _moneyService.EnsureCurrency(part.Price);
            return base.LoadingAsync(context, part);
        }
    }
}
