using OrchardCore.Commerce.Abstractions.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Settings;

public class AddressFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<AddressField>
{
    public override IDisplayResult Edit(ContentPartFieldDefinition model, BuildEditorContext context) =>
        Initialize<AddressPartFieldSettings>("AddressFieldSettings_Edit", viewModel =>
        {
            var settings = model.Settings.ToObject<AddressPartFieldSettings>();
            viewModel.Hint = settings.Hint;
        }).PlaceInContent();

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition model, UpdatePartFieldEditorContext context)
    {
        context.Builder.WithSettings(await context.CreateModelAsync<AddressPartFieldSettings>(Prefix));

        return await EditAsync(model, context);
    }
}
