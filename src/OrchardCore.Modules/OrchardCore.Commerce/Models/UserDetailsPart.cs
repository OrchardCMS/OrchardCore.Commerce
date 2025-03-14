using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Models;

public class UserDetailsPart : ContentPart
{
    public TextField PhoneNumber { get; set; } = new();
    public TextField VatNumber { get; set; } = new();
    public BooleanField IsCorporation { get; set; } = new();
}
