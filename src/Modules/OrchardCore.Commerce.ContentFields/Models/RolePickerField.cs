using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.ContentFields.Models;

public class RolePickerField : ContentField
{
    public IEnumerable<string> RoleNames { get; set; } = Enumerable.Empty<string>();
}
