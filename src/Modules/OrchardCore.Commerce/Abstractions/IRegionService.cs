using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// When implemented a service for handling regions.
/// </summary>
public interface IRegionService
{
    /// <summary>
    /// Gets the available regions from the site settings.
    /// </summary>
    /// <returns>A collection of <see cref="RegionInfo"/>.</returns>
    Task<IEnumerable<RegionInfo>> GetAvailableRegionsAsync();
}
