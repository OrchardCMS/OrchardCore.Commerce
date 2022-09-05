using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using static OrchardCore.Commerce.Constants.ContentTypes;

namespace OrchardCore.Commerce.Migrations;
public class ShoppingCartWidgetMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public ShoppingCartWidgetMigrations(IContentDefinitionManager contentDefinitionManager) =>
        _contentDefinitionManager = contentDefinitionManager;

    public int Create()
    {
        _contentDefinitionManager
            .AlterTypeDefinition(ShoppingCartWidget, type => type
            .WithPart(nameof(ShoppingCartWidgetPart))
            .Stereotype("Widget"));

        return 1;
    }
}
