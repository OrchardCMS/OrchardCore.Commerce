using OrchardCore.Commerce.ContentFields.Settings;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace OrchardCore.Commerce.Migrations;

/// <summary>
/// Adds the price part to the list of available parts.
/// </summary>
public class TaxMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public TaxMigrations(IContentDefinitionManager contentDefinitionManager) =>
        _contentDefinitionManager = contentDefinitionManager;

    public int Create()
    {
        _contentDefinitionManager
            .AlterPartDefinition<TaxPart>(builder => builder
                .Configure(part => part
                    .Attachable()
                    .Reusable()
                    .WithDescription("Adds tax information to a product."))
                .WithField(part => part.ProductTaxCode, field => field
                    .WithDisplayName("Product Tax Code"))
                .WithField(part => part.GrossPrice, field => field
                    .WithDisplayName("Gross Price")
                    .WithSettings(new PriceFieldSettings
                    {
                        Hint = "The price including local tax. If filled then Gross Price Tax Rate must be filled out as well.",
                    }))
                .WithField(part => part.GrossPriceRate, field => field
                    .WithDisplayName("Gross Price Tax Rate")
                    .WithSettings(new NumericFieldSettings
                    {
                        Hint = "The tax rate on the local gross price. If filled then Gross Price must be filled out as well.",
                        Minimum = 0,
                        Maximum = 100,
                    })));

        return 1;
    }
}
