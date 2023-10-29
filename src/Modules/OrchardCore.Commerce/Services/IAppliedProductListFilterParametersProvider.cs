using OrchardCore.Commerce.Models;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

/// <summary>
/// Provides a way to get the applied filter parameters for a product list.
/// </summary>
public interface IAppliedProductListFilterParametersProvider
{
    /// <summary>
    /// Gets the priority of this provider. The provider with the highest priority will be used.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Returns the applied filter parameters for the given product list.
    /// </summary>
    Task<ProductListFilterParameters> GetFilterParametersAsync(ProductListPart productList);
}