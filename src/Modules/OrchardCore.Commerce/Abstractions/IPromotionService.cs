using OrchardCore.Commerce.Models;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// A service that can apply promotions to a set of shopping cart items.
/// </summary>
public interface IPromotionService
{
    /// <summary>
    /// Applies promotions harvested from all promotion providers to shopping cart items, in order.
    /// </summary>
    /// <param name="context">The quantities and products to which promotions must be added.</param>
    Task<PromotionAndTaxProviderContext> AddPromotionsAsync(PromotionAndTaxProviderContext context);

    /// <summary>
    /// Checks if there is any applicable provider for the given <see cref="PromotionAndTaxProviderContext"/>.
    /// </summary>
    /// <param name="context">The <see cref="PromotionAndTaxProviderContext"/> which should be checked for applicable
    /// provider(s).</param>
    Task<bool> IsThereAnyApplicableProviderAsync(PromotionAndTaxProviderContext context);
}
