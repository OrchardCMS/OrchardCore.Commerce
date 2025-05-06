using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;
using static OrchardCore.Commerce.ContentFields.Constants.ResourceNames;

namespace OrchardCore.Commerce.ContentFields;

public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest = new();

    static ResourceManagementOptionsConfiguration() =>
        _manifest
            .DefineScript(CommerceRegions)
            .SetDependencies(JQuery)
            .SetUrl("~/OrchardCore.Commerce.ContentFields/js/commerce-regions.js")
            .SetVersion("1.0.0");

    public void Configure(ResourceManagementOptions options) => options.ResourceManifests.Add(_manifest);
}
