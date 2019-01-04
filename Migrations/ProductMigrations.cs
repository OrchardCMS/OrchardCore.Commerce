using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.Commerce.Indexes;

namespace OrchardCore.Commerce.Migrations
{
    /// <summary>
    /// Adds the product part to the list of available parts.
    /// </summary>
    public class ProductMigrations : DataMigration
    {
        IContentDefinitionManager _contentDefinitionManager;

        public ProductMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition("ProductPart", builder => builder
                .Attachable()
                .WithDescription("Makes a content item into a product."));

            SchemaBuilder.CreateMapIndexTable(nameof(ProductPartIndex), table => table
                .Column<string>("Sku", col => col.WithLength(128))
                .Column<string>("ContentItemId", c => c.WithLength(26))
            );

            SchemaBuilder.AlterTable(nameof(ProductPartIndex), table => table
                .CreateIndex("IDX_ProductPartIndex_Sku", "Sku")
            );

            return 1;
        }
    }
}