using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OrchardCore.Commerce.ViewModels;

public class ShoppingCartLineUpdateModel
{
    public int Quantity { get; set; }

    public string ProductSku { get; set; }

    [SuppressMessage(
        "Usage",
        "CA2227:Collection properties should be read only",
        Justification = "Setter is only used in the tests, allowed to keep them simpler.")]
    public IDictionary<string, string[]> Attributes { get; set; }
}
