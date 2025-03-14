using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// A service that updates the <see cref="ProductEstimationContext"/> used by the <see
/// cref="IShoppingCartHelpers.EstimateProductAsync"/> method.
/// </summary>
public interface IProductEstimationContextUpdater : ISortableUpdaterProvider<ProductEstimationContext>
{
}
