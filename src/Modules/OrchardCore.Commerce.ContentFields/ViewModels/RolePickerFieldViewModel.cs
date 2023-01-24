using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Commerce.ContentFields.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using System.Collections.Generic;

namespace OrchardCore.Commerce.ContentFields.ViewModels;

public class RolePickerFieldViewModel
{
    public string RoleNames { get; set; } = string.Empty;

    [BindNever]
    public RolePickerField Field { get; set; }

    [BindNever]
    public ContentPart Part { get; set; }

    [BindNever]
    public ContentPartFieldDefinition PartFieldDefinition { get; set; }

    [BindNever]
    public ContentTypePartDefinition TypePartDefinition { get; set; }

    [BindNever]
    public IList<VueMultiselectRoleViewModel> Roles { get; } = new List<VueMultiselectRoleViewModel>();

    public class VueMultiselectRoleViewModel
    {
        public string Id { get; set; }
        public bool IsEnabled { get; set; }
    }
}
