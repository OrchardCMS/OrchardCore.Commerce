using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.ContentManagement.Handlers;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Handlers;

public class TieredPricePartHandler : ContentPartHandler<TieredPricePart>
{
    private readonly IMoneyService _moneyService;

    public TieredPricePartHandler(IMoneyService moneyService) => _moneyService = moneyService;

    public override Task LoadingAsync(LoadContentContext context, TieredPricePart part)
    {
        if (part.TieredPrices != null)
        {
            foreach (var variantKey in part.TieredPrices.Keys)
            {
                part.TieredPrices[variantKey] = _moneyService.EnsureCurrency(part.TieredPrices[variantKey]);
            }
        }

        return base.LoadingAsync(context, part);
    }
}
