using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Commerce.Promotion.Models;
using OrchardCore.Commerce.Tax.Migrations;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;

namespace OrchardCore.Commerce.Promotion;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services) =>
        services.AddContentPart<DiscountPart>()
            .WithMigration<DiscountPartMigrations>();
}
