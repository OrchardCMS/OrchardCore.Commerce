using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.Commerce.Indexes;

namespace OrchardCore.Commerce.Migrations
{
    /// <summary>
    /// Adds the price part to the list of available parts.
    /// </summary>
    public class PriceMigrations : DataMigration
    {
        IContentDefinitionManager _contentDefinitionManager;

        public PriceMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition("PricePart", builder => builder
                .Attachable()
                .WithDescription("Adds a simple price to a product."));
            return 1;
        }
    }
}