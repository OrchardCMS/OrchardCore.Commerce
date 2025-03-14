using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Migrations;

/// <summary>
/// Adds the tiered price part to the list of available parts.
/// </summary>
public class TieredPriceMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public TieredPriceMigrations(IContentDefinitionManager contentDefinitionManager) =>
        _contentDefinitionManager = contentDefinitionManager;

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager
            .AlterPartDefinitionAsync(nameof(TieredPricePart), builder => builder
                .Attachable()
                .WithDescription("Adds tiered prices to a product based on quantity."));
        return 1;
    }
}
