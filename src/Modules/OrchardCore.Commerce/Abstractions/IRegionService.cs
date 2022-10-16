using OrchardCore.Commerce.AddressDataType;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// A service for accessing and customizing regions.
/// </summary>
public interface IRegionService
{
    /// <summary>
    /// Gets the available regions from the site settings.
    /// </summary>
    /// <returns>A collection of <see cref="RegionInfo"/>.</returns>
    Task<IEnumerable<Region>> GetAvailableRegionsAsync();
}
