using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.Commerce.Handlers;

public class PriceVariantsPartHandler : ContentPartHandler<PriceVariantsPart>
{
    private readonly IMoneyService _moneyService;

    public PriceVariantsPartHandler(IMoneyService moneyService) => _moneyService = moneyService;

    public override Task LoadingAsync(LoadContentContext context, PriceVariantsPart instance)
    {
        if (instance.Variants != null)
        {
            foreach (var variantKey in instance.Variants.Keys)
            {
                instance.Variants[variantKey] = _moneyService.EnsureCurrency(instance.Variants[variantKey]);
            }
        }

        return base.LoadingAsync(context, instance);
    }
}
