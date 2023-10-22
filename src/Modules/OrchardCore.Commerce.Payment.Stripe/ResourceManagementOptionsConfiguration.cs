using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;
using static OrchardCore.Commerce.Payment.Stripe.Constants.ResourceNames;

namespace OrchardCore.Commerce.Payment.Stripe;

public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest = new();

    static ResourceManagementOptionsConfiguration() =>
        _manifest
            .DefineScript(StripePaymentForm)
            .SetUrl(
                "~/OrchardCore.Commerce/js/stripe-payment-form.min.js",
                "~/OrchardCore.Commerce/js/stripe-payment-form.js")
            .SetVersion("1.0.0");

    public void Configure(ResourceManagementOptions options) => options.ResourceManifests.Add(_manifest);
}
