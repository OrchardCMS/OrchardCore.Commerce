using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace OrchardCore.Commerce.ViewModels;

public class RegionSettingsViewModel
{
    [BindNever]
    public IEnumerable<SelectListItem> Regions { get; set; }

    public IEnumerable<string> AllowedRegions { get; set; }
}
