using System.Text.Json.Nodes;
using OrchardCore.Commerce.Abstractions.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Settings;

public class AddressFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<AddressField>
{
    public override IDisplayResult Edit(ContentPartFieldDefinition model) =>
        Initialize<AddressPartFieldSettings>("AddressFieldSettings_Edit", viewModel =>
            {
                var settings = model.Settings.ToObject<AddressPartFieldSettings>();
                viewModel.Hint = settings.Hint;
            })
            .PlaceInContent();

    public override async Task<IDisplayResult> UpdateAsync(
        ContentPartFieldDefinition model,
        UpdatePartFieldEditorContext context)
    {
        var viewModel = new AddressPartFieldSettings();

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);
        context.Builder.WithSettings(viewModel);

        return await EditAsync(model, context);
    }
}
