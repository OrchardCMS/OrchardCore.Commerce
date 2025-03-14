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
        part.DefaultPrice = _moneyService.EnsureCurrency(part.DefaultPrice);

        return base.LoadingAsync(context, part);
    }
}
