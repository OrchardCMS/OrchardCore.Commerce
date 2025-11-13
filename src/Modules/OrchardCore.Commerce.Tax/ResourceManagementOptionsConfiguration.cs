using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Commerce.Tax;

public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest = new();

    static ResourceManagementOptionsConfiguration() =>
        _manifest
            .DefineScript("OrchardCore.Commerce.Tax.TaxRateEditor")
            .SetUrl("~/OrchardCore.Commerce.Tax/js/tax-rate-editor.js")
            .SetDependencies("vuejs:2")
            .SetVersion("1.0.0");

    public void Configure(ResourceManagementOptions options) => options.ResourceManifests.Add(_manifest);
}
