using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Commerce.ContentFields.Drivers;
using OrchardCore.Commerce.ContentFields.Models;
using OrchardCore.Commerce.Drivers;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Modules;

namespace OrchardCore.Commerce.ContentFields;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<TemplateOptions>(option =>
            option.MemberAccessStrategy.Register<PriceField>());

        services.AddScoped<IFieldsOnlyDisplayManager, FieldsOnlyDisplayManager>();

        // Price Field
        services.AddContentField<PriceField>()
            .UseDisplayDriver<PriceFieldDisplayDriver>();
        services.AddScoped<IContentPartFieldDefinitionDisplayDriver, PriceFieldSettingsDriver>();

        // Address Field
        services.AddContentField<AddressField>()
            .UseDisplayDriver<AddressFieldDisplayDriver>();
        services.AddScoped<IContentPartFieldDefinitionDisplayDriver, AddressFieldSettingsDriver>();
    }
}
