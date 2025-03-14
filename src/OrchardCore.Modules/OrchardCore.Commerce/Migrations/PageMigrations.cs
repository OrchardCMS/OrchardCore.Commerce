using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Migrations;

public class PageMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public PageMigrations(IContentDefinitionManager contentDefinitionManager) =>
        _contentDefinitionManager = contentDefinitionManager;

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterTypeDefinitionAsync("Page", type => type
            .Creatable()
            .Listable()
            .Draftable()
            .Versionable()
            .Securable()
            .WithTitlePart()
            .WithPart(nameof(AutoroutePart), settings =>
                settings.WithSettings(new AutoroutePartSettings
                {
                    ShowHomepageOption = true,
                    AllowCustomPath = true,
                })));

        return 1;
    }
}
