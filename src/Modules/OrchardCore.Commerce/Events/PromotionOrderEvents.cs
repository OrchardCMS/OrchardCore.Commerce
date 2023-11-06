using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Abstractions.ViewModels;
using OrchardCore.Commerce.Promotion.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Events;

public class PromotionOrderEvents : IOrderEvents
{
    public Task CreatedFreeAsync(OrderPart orderPart, ShoppingCart cart, ShoppingCartViewModel viewModel)
    {
        // Store the current applicable discount info, so they will be available in the future.
        orderPart.AdditionalData.SetDiscountsByProduct(viewModel
            .Lines
            .Where(line => line.AdditionalData.GetDiscounts().Any())
            .ToDictionary(
                line => line.ProductSku,
                line => line.AdditionalData.GetDiscounts()));

        return Task.CompletedTask;
    }
}
