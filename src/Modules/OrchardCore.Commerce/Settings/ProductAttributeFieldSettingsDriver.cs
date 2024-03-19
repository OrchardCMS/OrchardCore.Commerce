using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
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
    public override IDisplayResult Edit(ContentPartFieldDefinition model) =>
        Initialize(typeof(TSettings).Name + "_Edit", (Action<TSettings>)model.PopulateSettings)
            .PlaceInContent();

    public override async Task<IDisplayResult> UpdateAsync(
        ContentPartFieldDefinition model,
        UpdatePartFieldEditorContext context)
    {
        var viewModel = new TSettings();

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);
        context.Builder.WithSettings(viewModel);

        return Edit(model);
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
    public override IDisplayResult Edit(ContentPartFieldDefinition model) =>
        Initialize<TextProductAttributeSettingsViewModel>(
            nameof(TextProductAttributeFieldSettings) + "_Edit",
            viewModel =>
            {
                var settings = new TextProductAttributeFieldSettings();
                model.PopulateSettings(settings);
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
        var viewModel = new TextProductAttributeSettingsViewModel();
        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);
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
        return Edit(model);
    }
}
