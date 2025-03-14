using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Commerce.Inventory.Drivers;
using OrchardCore.Commerce.Inventory.Migrations;
using OrchardCore.Commerce.Inventory.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Modules;

namespace OrchardCore.Commerce.Inventory;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services) =>
        services.AddContentPart<InventoryPart>()
            .UseDisplayDriver<InventoryPartDisplayDriver>()
            .WithMigration<InventoryPartMigrations>();
}
