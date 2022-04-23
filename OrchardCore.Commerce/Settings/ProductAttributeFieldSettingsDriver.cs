using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Commerce.Settings;

public abstract class ProductAttributeFieldSettingsDriver<TFIeld, TSettings>
    : ContentPartFieldDefinitionDisplayDriver<TFIeld>
    where TFIeld : ProductAttributeField
    where TSettings : ProductAttributeFieldSettings, new()
{
    public override IDisplayResult Edit(ContentPartFieldDefinition model)
        => Initialize(typeof(TSettings).Name + "_Edit",
                (Action<TSettings>)(model => model.PopulateSettings<TSettings>(model)))
            .Location("Content");

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition model, UpdatePartFieldEditorContext context)
    {
        var model = new TSettings();
        await context.Updater.TryUpdateModelAsync(model, Prefix);
        context.Builder
            .WithSettings(model);
        return Edit(model);
    }
}

public class BooleanProductAttributeFieldSettingsDriver
    : ProductAttributeFieldSettingsDriver<BooleanProductAttributeField, BooleanProductAttributeFieldSettings>
{ }

public class NumericProductAttributeFieldSettingsDriver
    : ProductAttributeFieldSettingsDriver<NumericProductAttributeField, NumericProductAttributeFieldSettings>
{ }

public class TextProductAttributeFieldSettingsDriver
    : ProductAttributeFieldSettingsDriver<TextProductAttributeField, TextProductAttributeFieldSettings>
{
    public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        => Initialize<TextProductAttributeSettingsViewModel>(nameof(TextProductAttributeFieldSettings) + "_Edit",
            viewModel =>
            {
                var model = new TextProductAttributeFieldSettings();
                partFieldDefinition.PopulateSettings(model);
                viewModel.Hint = model.Hint;
                viewModel.DefaultValue = model.DefaultValue;
                viewModel.Required = model.Required;
                viewModel.Placeholder = model.Placeholder;
                viewModel.PredefinedValues = model.PredefinedValues != null ? string.Join("\r\n", model.PredefinedValues) : string.Empty;
                viewModel.RestrictToPredefinedValues = model.RestrictToPredefinedValues;
                viewModel.MultipleValues = model.MultipleValues;
            }).Location("Content");

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
    {
        var model = new TextProductAttributeSettingsViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);
        context.Builder
            .WithSettings(new TextProductAttributeFieldSettings
            {
                Hint = model.Hint,
                DefaultValue = model.DefaultValue,
                Required = model.Required,
                Placeholder = model.Placeholder,
                RestrictToPredefinedValues = model.RestrictToPredefinedValues,
                MultipleValues = model.MultipleValues,
                PredefinedValues = (model.PredefinedValues ?? string.Empty)
                    .Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(v => v.Trim())
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .ToArray(),
            });
        return Edit(partFieldDefinition);
    }
}
