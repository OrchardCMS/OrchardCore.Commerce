using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Commerce.Abstractions.Fields;
using OrchardCore.DisplayManagement.Views;
using System.Collections.Generic;

namespace OrchardCore.Commerce.ViewModels;

public class AddressFieldEditorViewModel : ShapeViewModel
{
    public const string ShapeType = "AddressFieldEditor";

    public IAddressField AddressField { get; set; }

    public bool HidePersonFields { get; set; }
    public bool HidePostalFields { get; set; }

    public string CityName { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Regions { get; set; }

    [BindNever]
    // We don't want to mess with filling up this every time also, it's just a view-model so it's safe.
#pragma warning disable CA2227 // Collection properties should be read only
    public IDictionary<string, IDictionary<string, string>> Provinces { get; set; } =
#pragma warning restore CA2227 // Collection properties should be read only
        new Dictionary<string, IDictionary<string, string>>();

    public AddressFieldEditorViewModel() => Metadata.Type = ShapeType;
}
