using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Commerce.Tax.Handlers;
using OrchardCore.Commerce.Tax.Migrations;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;

namespace OrchardCore.Commerce.Tax;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services) =>
        services.AddContentPart<TaxPart>()
            .WithMigration<TaxPartMigrations>()
            .AddHandler<TaxPartHandler>();
}
