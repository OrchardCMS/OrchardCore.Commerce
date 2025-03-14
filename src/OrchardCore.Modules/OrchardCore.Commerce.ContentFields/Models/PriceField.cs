using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.ContentFields.Models;

public class PriceField : ContentField
{
    public Amount Amount { get; set; }
}
