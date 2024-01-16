using Fluid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Abstractions.Fields;
using OrchardCore.Commerce.ContentFields.Drivers;
using OrchardCore.Commerce.ContentFields.Models;
using OrchardCore.Commerce.Drivers;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Commerce.ContentFields;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<ResourceManagementOptions>, ResourceManagementOptionsConfiguration>();
        services.AddScoped<IFieldsOnlyDisplayManager, FieldsOnlyDisplayManager>();

        services.Configure<TemplateOptions>(option =>
            option.MemberAccessStrategy.Register<PriceField>());

        // Price Field
        services.AddContentField<PriceField>()
            .UseDisplayDriver<PriceFieldDisplayDriver>();
        services.AddScoped<IContentPartFieldDefinitionDisplayDriver, PriceFieldSettingsDriver>();

        // Address Field
        services.AddContentField<AddressField>()
            .UseDisplayDriver<AddressFieldDisplayDriver>();
        services.AddScoped<IContentPartFieldDefinitionDisplayDriver, AddressFieldSettingsDriver>();
        services.AddScoped<IShapeTableProvider, AddressUpdaterShapeTableProvider>();
    }
}
