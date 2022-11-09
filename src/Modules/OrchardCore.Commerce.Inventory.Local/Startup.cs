using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Commerce.Inventory.Local.Migrations;
using OrchardCore.Commerce.Inventory.Local.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;

namespace OrchardCore.Commerce.Inventory.Local;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services) =>
        services.AddContentPart<InventoryPart>()
            .WithMigration<InventoryPartMigrations>();
}
