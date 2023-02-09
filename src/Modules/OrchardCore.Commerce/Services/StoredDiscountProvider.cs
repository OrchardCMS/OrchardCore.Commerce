using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using System.Linq;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Services.DiscountProvider;

namespace OrchardCore.Commerce.Services;

public class StoredDiscountProvider : IPromotionProvider
{
    public int Order => int.MinValue;

    public Task<PromotionAndTaxProviderContext> UpdateAsync(PromotionAndTaxProviderContext model) =>
        model.UpdateAsync((item, purchaseDateTime) =>
            Task.FromResult(ApplyPromotionToShoppingCartItem(item, purchaseDateTime, item.Discounts)));

    public Task<bool> IsApplicableAsync(PromotionAndTaxProviderContext model) =>
        Task.FromResult(model.Stored && model.Items.Any(item => item.Discounts?.Any() == true));
}
