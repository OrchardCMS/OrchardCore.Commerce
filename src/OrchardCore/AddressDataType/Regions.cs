using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace InternationalAddress
{
    public static class Regions
    {
        /// <summary>
        /// The list of regions.
        /// </summary>
        public static readonly IList<RegionInfo> All
            = CultureInfo
                .GetCultures(CultureTypes.SpecificCultures)
                .Select(culture => new RegionInfo(culture.LCID))
                .Where(region => region.TwoLetterISORegionName.Length == 2) // Filter out world and other 3-digit regions
                .Distinct()
                .ToList();

        /// <summary>
        /// Maps region names to two-letter ISO region codes.
        /// </summary>
        public static readonly IDictionary<string, string> RegionCodes
            = All.ToDictionary(
                    region => region.DisplayName,
                    region => region.TwoLetterISORegionName,
                    StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Maps two-letter regions codes to 
        /// </summary>
        public static readonly IDictionary<string, IDictionary<string, string>> Provinces
            = new Dictionary<string, IDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
            { // TODO: complete this list with other countries that have regions
                { "US" , new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                      { "AL", "Alabama" },
                      { "AK", "Alaska" },
                      { "AZ", "Arizona" },
                      { "AR", "Arkansas" },
                      { "CA", "California" },
                      { "CO", "Colorado" },
                      { "CT", "Connecticut" },
                      { "DE", "Delaware" },
                      { "DC", "District Of Columbia" },
                      { "FL", "Florida" },
                      { "GA", "Georgia" },
                      { "HI", "Hawaii" },
                      { "ID", "Idaho" },
                      { "IL", "Illinois" },
                      { "IN", "Indiana" },
                      { "IA", "Iowa" },
                      { "KS", "Kansas" },
                      { "KY", "Kentucky" },
                      { "LA", "Louisiana" },
                      { "ME", "Maine" },
                      { "MD", "Maryland" },
                      { "MA", "Massachusetts" },
                      { "MI", "Michigan" },
                      { "MN", "Minnesota" },
                      { "MS", "Mississippi" },
                      { "MO", "Missouri" },
                      { "MT", "Montana" },
                      { "NE", "Nebraska" },
                      { "NV", "Nevada" },
                      { "NH", "New Hampshire" },
                      { "NJ", "New Jersey" },
                      { "NM", "New Mexico" },
                      { "NY", "New York" },
                      { "NC", "North Carolina" },
                      { "ND", "North Dakota" },
                      { "OH", "Ohio" },
                      { "OK", "Oklahoma" },
                      { "OR", "Oregon" },
                      { "PA", "Pennsylvania" },
                      { "RI", "Rhode Island" },
                      { "SC", "South Carolina" },
                      { "SD", "South Dakota" },
                      { "TN", "Tennessee" },
                      { "TX", "Texas" },
                      { "UT", "Utah" },
                      { "VT", "Vermont" },
                      { "VA", "Virginia" },
                      { "WA", "Washington" },
                      { "WV", "West Virginia" },
                      { "WI", "Wisconsin" },
                      { "WY", "Wyoming" }
                    }
                },
                { "CA" , new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                      { "AB", "Alberta" },
                      { "BC", "British Columbia" },
                      { "MB", "Manitoba" },
                      { "NB", "New Brunswick" },
                      { "NL", "Newfoundland and Labrador" },
                      { "NS", "Nova Scotia" },
                      { "NT", "Northwest Territories" },
                      { "NU", "Nunavut" },
                      { "ON", "Ontario" },
                      { "PE", "Prince Edward Island" },
                      { "QC", "Quebec" },
                      { "SK", "Saskatchewan" },
                      { "YT", "Yukon" }
                    }
                }
            };
    }
}
