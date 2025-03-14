using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Migrations;

/// <summary>
/// Adds the price variants part to the list of available parts.
/// </summary>
public class PriceVariantsMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public PriceVariantsMigrations(IContentDefinitionManager contentDefinitionManager) =>
        _contentDefinitionManager = contentDefinitionManager;

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager
            .AlterPartDefinitionAsync(nameof(PriceVariantsPart), builder => builder
                .Attachable()
                .WithDescription("A product variants prices based on predefined attributes."));
        return 1;
    }
}
