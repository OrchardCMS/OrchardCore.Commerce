using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Globalization;

namespace OrchardCore.Commerce.ViewModels;

public class RegionSettingsViewModel
{
    [BindNever]
    public IEnumerable<RegionInfo> Regions { get; set; }

    public IEnumerable<RegionInfo> AllowedRegions { get; set; }
}
