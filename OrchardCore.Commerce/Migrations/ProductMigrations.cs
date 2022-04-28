using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace OrchardCore.Commerce.Migrations;

/// <summary>
/// Adds the product part to the list of available parts.
/// </summary>
public class ProductMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public ProductMigrations(IContentDefinitionManager contentDefinitionManager) =>
        _contentDefinitionManager = contentDefinitionManager;

    public int Create()
    {
        _contentDefinitionManager
            .AlterPartDefinition(nameof(ProductPart), builder => builder
                .Attachable()
                .WithDescription("Makes a content item into a product."));

        SchemaBuilder
            .CreateMapIndexTable<ProductPartIndex>(table => table
                .Column<string>(nameof(ProductPartIndex.Sku), column => column.WithLength(128))
                .Column<string>(nameof(ProductPartIndex.ContentItemId), column => column.WithLength(26)));

        SchemaBuilder
            .AlterTable(nameof(ProductPartIndex), table => table
                .CreateIndex(
                    $"IDX_{nameof(ProductPartIndex)}_{nameof(ProductPartIndex.Sku)}",
                    nameof(ProductPartIndex.Sku)));

        return 1;
    }
}
