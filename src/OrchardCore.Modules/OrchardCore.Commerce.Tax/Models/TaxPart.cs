using OrchardCore.Commerce.ContentFields.Models;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Tax.Models;

public class TaxPart : ContentPart
{
    public TextField ProductTaxCode { get; set; } = new();
    public PriceField GrossPrice { get; set; } = new();
    public NumericField TaxRate { get; set; } = new();
}
