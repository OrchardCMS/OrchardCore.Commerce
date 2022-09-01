using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Models;

public class TaxPart : ContentPart
{
    public TextField ProductTaxCode { get; set; } = new();
    public Amount? GrossPrice { get; set; }
    public NumericField GrossPriceRate { get; set; } = new();
}
