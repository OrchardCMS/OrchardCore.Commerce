using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;

namespace OrchardCore.Commerce.Migrations
{
    /// <summary>
    /// Adds price book selection based on user role.
    /// </summary>
    public class PriceBookByRoleMigrations : DataMigration
    {
        IContentDefinitionManager _contentDefinitionManager;

        public PriceBookByRoleMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition("PriceBookByRolePart", builder => builder
                .Attachable(false)
                .WithDescription("Used to determine appropriate price book based on role."));

            _contentDefinitionManager.AlterTypeDefinition("PriceBookByRole", builder => builder
                .DisplayedAs("Price Book By Role")
                .Draftable()
                .Versionable()
                .WithPart("PriceBookByRolePart")
                .WithPart("PriceBookRulePart"));
            return 1;
        }
    }
}