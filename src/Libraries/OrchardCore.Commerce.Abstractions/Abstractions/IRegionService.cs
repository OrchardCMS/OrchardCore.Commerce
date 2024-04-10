using OrchardCore.Commerce.AddressDataType;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions.Abstractions;

/// <summary>
/// A service for accessing and customizing regions.
/// </summary>
public interface IRegionService
{
    /// <summary>
    /// Gets all regions and applies localization to their <see cref="Region.DisplayName"/> properties.
    /// </summary>
    IEnumerable<Region> GetAllRegions();

    /// <summary>
    /// Gets the available regions from the site settings.
    /// </summary>
    /// <returns>
    /// A collection of <see cref="RegionInfo"/> where the <see cref="RegionInfo.DisplayName"/> entries are localized.
    /// </returns>
    Task<IEnumerable<Region>> GetAvailableRegionsAsync();

    /// <summary>
    /// Return all provinces/states/subdivisions of a region.
    /// </summary>
    /// <param name="twoLetterCode">The two letter ISO code of the country/region whose provinces are requested.</param>
    /// <returns>
    /// A dictionary whose keys are the province identifier codes (e.g. the two letter codes of US states) and the
    /// values are the full names, localized.
    /// </returns>
    Task<IDictionary<string, string>> GetProvincesAsync(string twoLetterCode);

    /// <summary>
    /// Returns the values of <see cref="GetProvincesAsync"/> for every region that has provinces.
    /// </summary>
    /// <returns>
    /// A dictionary whose keys are the two letter ISO codes that can return non-empty results when passed to  <see
    /// cref="GetProvincesAsync"/>. The value a dictionary equivalent to such a call.
    /// </returns>
    Task<IDictionary<string, IDictionary<string, string>>> GetAllProvincesAsync();
}
