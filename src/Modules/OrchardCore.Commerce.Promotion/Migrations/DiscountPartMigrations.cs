using OrchardCore.Commerce.ContentFields.Settings;
using OrchardCore.Commerce.Promotion.Models;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace OrchardCore.Commerce.Tax.Migrations;

/// <summary>
/// Adds the discount part to the list of available parts.
/// </summary>
public class DiscountPartMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public DiscountPartMigrations(IContentDefinitionManager contentDefinitionManager) =>
        _contentDefinitionManager = contentDefinitionManager;

    public int Create()
    {
        _contentDefinitionManager
            .AlterPartDefinition<DiscountPart>(builder => builder
                .Configure(part => part
                    .Attachable()
                    .Reusable()
                    .WithDescription("Adds configurable discount to the product."))
                .WithField(part => part.DiscountPercentage, part => part
                    .WithDisplayName("Discount Percentage")
                    .WithSettings(new NumericFieldSettings
                    {
                        Hint = "The discount percentage that is applied to the product. Out of discount percentage " +
                            "and discount amount only one can be used.",
                        Minimum = 0,
                        Maximum = 100,
                    }))
                .WithField(part => part.DiscountAmount, part => part
                    .WithDisplayName("Discount Amount")
                    .WithSettings(new PriceFieldSettings
                    {
                        Hint = "The discount amount that is subtracted from the product's price. Out of discount " +
                            "percentage and discount amount only one can be used.",
                    }))
                .WithField(part => part.BeginningUtc, part => part
                    .WithDisplayName("Beginning Utc")
                    .WithSettings(new DateTimeFieldSettings
                    {
                        Hint = "The beginning date and time of the discount.",
                    }))
                .WithField(part => part.ExpirationUtc, part => part
                    .WithDisplayName("Expiration Utc")
                    .WithSettings(new DateTimeFieldSettings
                    {
                        Hint = "The expiration date and time of the discount.",
                    }))
                .WithField(part => part.MaximumProducts, part => part
                    .WithDisplayName("Maximum Products")
                    .WithSettings(new NumericFieldSettings
                    {
                        Hint = "The amount of maximum products that the discount can be applied to. If the quantity " +
                            "is higher than this, the discount is ignored. Type 0 for infinite products.",
                    }))
                .WithField(part => part.MinimumProducts, part => part
                    .WithDisplayName("Minimum Products")
                    .WithSettings(new NumericFieldSettings
                    {
                        Hint = "The amount of minimum products that the discount can be applied to. If the quantity " +
                            "is lower than this, the discount is ignored.",
                    }))
            );

        return 1;
    }
}
