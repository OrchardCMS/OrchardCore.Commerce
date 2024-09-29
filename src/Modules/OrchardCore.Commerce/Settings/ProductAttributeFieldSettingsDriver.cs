using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Settings;

public abstract class ProductAttributeFieldSettingsDriver<TField, TSettings>
    : ContentPartFieldDefinitionDisplayDriver<TField>
    where TField : ProductAttributeField
    where TSettings : ProductAttributeFieldSettings, new()
{
    public override IDisplayResult Edit(ContentPartFieldDefinition model, BuildEditorContext context) =>
        Initialize<TSettings>(typeof(TSettings).Name + "_Edit", model.CopySettingsTo)
            .PlaceInContent();

    public override async Task<IDisplayResult> UpdateAsync(
        ContentPartFieldDefinition model,
        UpdatePartFieldEditorContext context)
    {
        var viewModel = await context.CreateModelAsync<TSettings>(Prefix);
        context.Builder.WithSettings(viewModel);

        return await EditAsync(model, context);
    }
}

public class BooleanProductAttributeFieldSettingsDriver
    : ProductAttributeFieldSettingsDriver<BooleanProductAttributeField, BooleanProductAttributeFieldSettings>
{
}

public class NumericProductAttributeFieldSettingsDriver
    : ProductAttributeFieldSettingsDriver<NumericProductAttributeField, NumericProductAttributeFieldSettings>
{
}

public class TextProductAttributeFieldSettingsDriver
    : ProductAttributeFieldSettingsDriver<TextProductAttributeField, TextProductAttributeFieldSettings>
{
    public override IDisplayResult Edit(ContentPartFieldDefinition model, BuildEditorContext context) =>
        Initialize<TextProductAttributeSettingsViewModel>(
            nameof(TextProductAttributeFieldSettings) + "_Edit",
            viewModel =>
            {
                var settings = model.GetSettings<TextProductAttributeFieldSettings>();
                viewModel.Hint = settings.Hint;
                viewModel.DefaultValue = settings.DefaultValue;
                viewModel.Required = settings.Required;
                viewModel.Placeholder = settings.Placeholder;
                viewModel.PredefinedValues = settings.PredefinedValues != null
                    ? string.Join("\r\n", settings.PredefinedValues)
                    : string.Empty;
                viewModel.RestrictToPredefinedValues = settings.RestrictToPredefinedValues;
                viewModel.MultipleValues = settings.MultipleValues;
            }).PlaceInContent();

    private static readonly char[] Separators = ['\r', '\n'];

    public override async Task<IDisplayResult> UpdateAsync(
        ContentPartFieldDefinition model,
        UpdatePartFieldEditorContext context)
    {
        var viewModel = await context.CreateModelAsync<TextProductAttributeSettingsViewModel>(Prefix);
        context.Builder
            .WithSettings(new TextProductAttributeFieldSettings
            {
                Hint = viewModel.Hint,
                DefaultValue = viewModel.DefaultValue,
                Required = viewModel.Required,
                Placeholder = viewModel.Placeholder,
                RestrictToPredefinedValues = viewModel.RestrictToPredefinedValues,
                MultipleValues = viewModel.MultipleValues,
                PredefinedValues = (viewModel.PredefinedValues ?? string.Empty)
                    .Split(Separators, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .ToList(),
            });

        return await EditAsync(model, context);
    }
}
