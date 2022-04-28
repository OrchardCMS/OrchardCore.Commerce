using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace OrchardCore.Commerce.Migrations;

/// <summary>
/// Adds the price part to the list of available parts.
/// </summary>
public class PriceMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public PriceMigrations(IContentDefinitionManager contentDefinitionManager) =>
        _contentDefinitionManager = contentDefinitionManager;

    public int Create()
    {
        _contentDefinitionManager
            .AlterPartDefinition(nameof(PricePart), builder => builder
                .Attachable()
                .Reusable()
                .WithDescription("Adds a simple price to a product."));
        return 1;
    }
}
