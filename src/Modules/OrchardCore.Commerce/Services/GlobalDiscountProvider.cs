using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class GlobalDiscountProvider : IPromotionProvider
{
    public int Order => 1;

    public Task<PromotionAndTaxProviderContext> UpdateAsync(PromotionAndTaxProviderContext model) =>
        throw new System.NotImplementedException();

    public Task<bool> IsApplicableAsync(PromotionAndTaxProviderContext model) =>
        throw new System.NotImplementedException();

}
