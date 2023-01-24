using OrchardCore.Commerce.ContentFields.Models;
using OrchardCore.Commerce.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Security.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.ContentFields.Drivers;

public class RolePickerFieldDisplayDriver : ContentFieldDisplayDriver<RolePickerField>
{
    private readonly IRoleService _roleService;

    public RolePickerFieldDisplayDriver(IRoleService roleService) =>
        _roleService = roleService;

    public override IDisplayResult Edit(RolePickerField field, BuildFieldEditorContext context) =>
        Initialize<RolePickerFieldViewModel>(GetEditorShapeType(context), async model =>
        {
            model.Field = field;
            model.Part = context.ContentPart;
            model.PartFieldDefinition = context.PartFieldDefinition;
            model.TypePartDefinition = context.TypePartDefinition;

            var roleNames = new HashSet<string>(field.RoleNames?.AsList() ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var allRoleNames = await _roleService.GetRoleNamesAsync();

            model.Roles.AddRange(allRoleNames
                .Select(role => new RolePickerFieldViewModel.VueMultiselectRoleViewModel
                {
                    Id = role,
                    IsEnabled = roleNames.Contains(role),
                })
                .OrderBy(role => role.Id));

            model.RoleNames = string.Join(",", model.Roles.Select(role => role.Id));
        });

    public override async Task<IDisplayResult> UpdateAsync(RolePickerField field, IUpdateModel updater, UpdateFieldEditorContext context)
    {
        var viewModel = new RolePickerFieldViewModel();

        if (await updater.TryUpdateModelAsync(viewModel, Prefix, model => model.RoleNames))
        {
            field.RoleNames = viewModel.RoleNames.SplitByCommas();
        }

        return await EditAsync(field, context);
    }
}
