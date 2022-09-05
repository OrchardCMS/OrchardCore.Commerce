using OrchardCore.Commerce.ContentFields.Settings;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace OrchardCore.Commerce.Tax.Migrations;

/// <summary>
/// Adds the price part to the list of available parts.
/// </summary>
public class TaxPartMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public TaxPartMigrations(IContentDefinitionManager contentDefinitionManager) =>
        _contentDefinitionManager = contentDefinitionManager;

    public int Create()
    {
        _contentDefinitionManager
            .AlterPartDefinition<TaxPart>(builder => builder
                .Configure(part => part
                    .Attachable()
                    .Reusable()
                    .WithDescription("Adds tax information usable for local VAT or sales tax."))
                .WithField(part => part.ProductTaxCode, part => part
                    .WithDisplayName("Tax Code")
                    .WithSettings(new TextFieldSettings
                    {
                        Hint = "The ID that identifies the product of product category for taxing purposes.",
                    }))
                .WithField(part => part.GrossPrice, part => part
                    .WithDisplayName("Gross Price")
                    .WithSettings(new PriceFieldSettings
                    {
                        Hint = "The price with tax. If specified along with the Gross Price Rate, then Price content " +
                               "part is updated to the calculated net value on publish..",
                    }))
                .WithField(part => part.GrossPriceRate, part => part
                    .WithDisplayName("Gross Price Rate")
                    .WithSettings(new NumericFieldSettings
                    {
                        Hint = "The tax percentage of the net price which is added to get the gross price.",
                        Minimum = 0,
                        Maximum = 100,
                    }))
            );

        return 1;
    }
}
