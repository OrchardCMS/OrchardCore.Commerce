using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.ContentManagement.Handlers;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Handlers;

public class PriceVariantsPartHandler : ContentPartHandler<PriceVariantsPart>
{
    private readonly IMoneyService _moneyService;

    public PriceVariantsPartHandler(IMoneyService moneyService) => _moneyService = moneyService;

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
