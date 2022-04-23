using System.Threading.Tasks;
using OrchardCore.Commerce.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Commerce.Settings;

public class AddressFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<AddressField>
{
    public override IDisplayResult Edit(ContentPartFieldDefinition model)
        => Initialize(
            "AddressFieldSettings_Edit",
            (System.Action<AddressPartFieldSettings>)model.PopulateSettings)
            .Location("Content");

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition model, UpdatePartFieldEditorContext context)
    {
        var viewModel = new AddressPartFieldSettings();

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);
        context.Builder.WithSettings(viewModel);

        return await EditAsync(model, context);
    }
}
