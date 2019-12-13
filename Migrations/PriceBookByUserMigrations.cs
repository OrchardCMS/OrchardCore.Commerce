using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.Commerce.Indexes;

namespace OrchardCore.Commerce.Migrations
{
    /// <summary>
    /// Adds price book selection based on user.
    /// </summary>
    public class PriceBookByUserMigrations : DataMigration
    {
        IContentDefinitionManager _contentDefinitionManager;

        public PriceBookByUserMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            // Content Parts
            _contentDefinitionManager.AlterPartDefinition("PriceBookByUserPart", builder => builder
                .Attachable(false)
                .WithDescription("Used to determine appropriate price book based on user."));

            // Content Types
            _contentDefinitionManager.AlterTypeDefinition("PriceBookByUser", builder => builder
                .DisplayedAs("Price Book By User")
                .Draftable()
                .Versionable()
                .WithPart("PriceBookByUserPart")
                .WithPart("PriceBookRulePart"));

            return 1;
        }
    }
}