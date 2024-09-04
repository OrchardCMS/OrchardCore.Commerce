using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Models;

#pragma warning disable S2333 // Redundant modifiers should not be used
public partial class UserDetailsPart : ContentPart
#pragma warning restore S2333 // Redundant modifiers should not be used
{
    public TextField PhoneNumber { get; set; } = new();
    public TextField VatNumber { get; set; } = new();
    public BooleanField IsCorporation { get; set; } = new();
}
