using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;
using static OrchardCore.Commerce.Payment.Constants.ResourceNames;

namespace OrchardCore.Commerce.Payment;

public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest = new();

    static ResourceManagementOptionsConfiguration() =>
        _manifest
            .DefineStyle(PaymentForm)
            .SetUrl(
                "~/OrchardCore.Commerce.Payment/css/payment-form.min.css",
                "~/OrchardCore.Commerce.Payment/css/payment-form.css")
            .SetVersion("1.0.0");

    public void Configure(ResourceManagementOptions options) => options.ResourceManifests.Add(_manifest);
}
