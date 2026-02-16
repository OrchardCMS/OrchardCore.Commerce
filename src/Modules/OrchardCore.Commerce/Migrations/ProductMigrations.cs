using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentFields.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Media.Fields;
using OrchardCore.Media.Settings;
using System.Threading.Tasks;
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

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager
            .AlterPartDefinitionAsync(nameof(ProductPart), builder => builder
                .WithField(nameof(ProductPart.ProductImage), field => field
                    .OfType(nameof(MediaField))
                    .WithDisplayName("Product Image")
                    .WithSettings(new MediaFieldSettings { Multiple = false }))
                .Attachable()
                .WithDescription("Makes a content item into a product."));

        await SchemaBuilder
            .CreateMapIndexTableAsync<ProductPartIndex>(table => table
                .Column<string>(nameof(ProductPartIndex.Sku), column => column.WithLength(128))
                .Column<string>(nameof(ProductPartIndex.ContentItemId), column => column.WithLength(26)));

        await SchemaBuilder
            .AlterTableAsync(nameof(ProductPartIndex), table => table
                .CreateIndex(
                    $"IDX_{nameof(ProductPartIndex)}_{nameof(ProductPartIndex.Sku)}",
                    nameof(ProductPartIndex.Sku)));

        return 2;
    }

    public async Task<int> UpdateFrom1Async()
    {
        await _contentDefinitionManager
            .AlterPartDefinitionAsync<ProductPart>(builder => builder
                .WithField(part => part.ProductImage, field => field
                    .WithDisplayName("Product Image")
                    .WithSettings(new MediaFieldSettings { Multiple = false })));
        return 2;
    }

    public async Task<int> UpdateFrom2Async()
    {
        await SchemaBuilder
            .AlterTableAsync(nameof(ProductPartIndex), table => {
                table.AddColumn<System.DateTime?>(nameof(ProductPartIndex.StartTimeUtc));
                table.AddColumn<System.DateTime?>(nameof(ProductPartIndex.EndTimeUtc));
            });

        await SchemaBuilder
            .AlterTableAsync(nameof(ProductPartIndex), table => table
                .CreateIndex(
                    $"IDX_{nameof(ProductPartIndex)}_TimeBased",
                    new[] 
                    { 
                        nameof(ProductPartIndex.StartTimeUtc),
                        nameof(ProductPartIndex.EndTimeUtc)
                    }));

        return 3;
    }

    public async Task<int> UpdateFrom3Async()
    {
        // Add DateTimeField fields to ProductPart for UI editing
        await _contentDefinitionManager
            .AlterPartDefinitionAsync<ProductPart>(builder => builder
                .WithField(part => part.StartTimeUtc, field => field
                    .WithDisplayName("Product Start Time")
                    .WithSettings(new DateTimeFieldSettings
                    {
                        Hint = "The date and time (UTC) from which the product becomes visible and available for purchase. Leave empty for immediate availability.",
                    }))
                .WithField(part => part.EndTimeUtc, field => field
                    .WithDisplayName("Product End Time")
                    .WithSettings(new DateTimeFieldSettings
                    {
                        Hint = "The date and time (UTC) until which the product remains visible and available for purchase. Leave empty for indefinite availability.",
                    })));

        return 4;
    }
}
