using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.Data.Migration;
using static OrchardCore.Commerce.Constants.ContentTypes;

namespace OrchardCore.Commerce.Migrations;

/// <summary>
/// Adds the MvcTitle content type to render titles on MVC views the same way contents are rendered on the theme.
/// </summary>
public class MvcTitleMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public MvcTitleMigrations(IContentDefinitionManager contentDefinitionManager) =>
        _contentDefinitionManager = contentDefinitionManager;

    public int Create()
    {
        _contentDefinitionManager.AlterTypeDefinition(MvcTitle, type => type
            .NoAbilities()
            .WithTitlePart());

        return 1;
    }
}
