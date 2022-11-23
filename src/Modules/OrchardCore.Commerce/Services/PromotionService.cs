using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

/// <summary>
/// A promotion service that asks all available promotion providers to apply promotions to a
/// <see cref="PromotionAndTaxProviderContext"/>.
/// </summary>
public class PromotionService : IPromotionService
{
    private readonly IEnumerable<IPromotionProvider> _promotionProviders;

    public PromotionService(IEnumerable<IPromotionProvider> promotionProviders) =>
        _promotionProviders = promotionProviders;

    public async Task<PromotionAndTaxProviderContext> AddPromotionsAsync(PromotionAndTaxProviderContext context)
    {
        var providers = await _promotionProviders
            .OrderBy(provider => provider.Order)
            .WhereAsync(provider => provider.IsApplicableAsync(context));

        foreach (var promotionProvider in providers)
        {
            var result = await promotionProvider.UpdateAsync(context);
            context = result;
        }

        return context;
    }

    public Task<bool> IsThereAnyApplicableProviderAsync(PromotionAndTaxProviderContext context) =>
         _promotionProviders.AnyAsync(provider => provider.IsApplicableAsync(context));
}
