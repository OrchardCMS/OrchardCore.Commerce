using OrchardCore.Commerce.ContentFields.Settings;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Payment.Stripe.Models;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Flows.Models;
using OrchardCore.Title.Models;
using System.Threading.Tasks;
using static Lombiq.HelpfulLibraries.OrchardCore.Contents.ContentFieldEditorEnums.TextFieldEditors;
using static OrchardCore.Commerce.Payment.Stripe.Constants.ContentTypes;
using static OrchardCore.Commerce.Payment.Stripe.Constants.PricePeriods;

namespace OrchardCore.Commerce.Payment.Stripe.Migrations;

public class StripeProductMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public StripeProductMigrations(IContentDefinitionManager contentDefinitionManager) =>
        _contentDefinitionManager = contentDefinitionManager;

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync<StripePricePart>(builder => builder
            .WithField(part => part.PriceId, field => field
                .WithDisplayName("Price Id")
                .WithDescription("Stripe's price Id."))
            .WithField(part => part.Name)
            .WithField(part => part.Price, field => field
                .WithSettings(new PriceFieldSettings
                {
                    Required = true,
                    Hint = "The price of the product set in Stripe.",
                }))
            .WithField(part => part.Period, field => field
                .WithEditor(nameof(PredefinedList))
                .WithSettings(new TextFieldPredefinedListEditorSettings
                {
                    Options =
                    [
                        new ListValueOption { Name = Daily, Value = Daily },
                        new ListValueOption { Name = Weekly, Value = Weekly },
                        new ListValueOption { Name = Monthly, Value = Monthly },
                        new ListValueOption { Name = Yearly, Value = Yearly },
                        new ListValueOption { Name = Custom, Value = Custom },
                    ],
                    DefaultValue = Monthly,
                    Editor = EditorOption.Dropdown,
                })));

        await _contentDefinitionManager.AlterTypeDefinitionAsync(StripePrice, type => type
            .Listable()
            .Securable()
            .WithPart(nameof(StripePricePart)));

        await _contentDefinitionManager.AlterPartDefinitionAsync<StripeProductFeaturePart>(builder => builder
            .WithField(part => part.FeatureName, field => field
                .WithDisplayName("Feature name")
                .WithDescription("Short feature description")));

        await _contentDefinitionManager.AlterTypeDefinitionAsync(StripeProductFeature, type => type
            .Listable()
            .Securable()
            .WithPart(nameof(StripeProductFeaturePart)));

        await _contentDefinitionManager.AlterPartDefinitionAsync<StripeProductPart>(builder => builder
            .WithField(part => part.StripeProductId, field => field.WithDisplayName("Stripe Product Id")));

        await _contentDefinitionManager.AlterTypeDefinitionAsync(StripeProduct, type => type
            .Listable()
            .Creatable()
            .Securable()
            .WithPart(nameof(TitlePart), part => part
                .WithDisplayName("Name")
                .WithSettings(new TitlePartSettings
                {
                    Options = TitlePartOptions.EditableRequired,
                }))
            .WithPart(nameof(StripeProductPart))
            .WithPart(nameof(ProductPart))
            .WithPart(nameof(FeatureCollectionPart), nameof(BagPart), part => part
                .WithDescription("Collection of features.")
                .WithDisplayName("Features")
                .WithSettings(new BagPartSettings
                {
                    ContainedContentTypes =
                    [
                        StripeProductFeature,
                    ],
                }))
            .WithPart(nameof(PriceCollectionPart), nameof(BagPart), part => part
                .WithDescription("Collection of prices.")
                .WithDisplayName("Prices")
                .WithSettings(new BagPartSettings
                {
                    ContainedContentTypes =
                    [
                        StripePrice,
                    ],
                })));

        return 1;
    }
}
