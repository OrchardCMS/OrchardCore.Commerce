using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;
using System.Diagnostics.CodeAnalysis;
using static OrchardCore.Commerce.Constants.ResourceNames;

namespace OrchardCore.Commerce;

public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest = new();

    [SuppressMessage(
        "Minor Code Smell",
        "S1192:String literals should not be duplicated",
        Justification = "Version strings should not be deduplicated when they aren't related.")]
    static ResourceManagementOptionsConfiguration()
    {
        _manifest
            .DefineStyle(CreditCardForm)
            .SetUrl(
                "~/OrchardCore.Commerce/css/credit-card-form.min.css",
                "~/OrchardCore.Commerce/css/credit-card-form.css")
            .SetVersion("1.0.0");

        _manifest
            .DefineStyle(ShoppingCart)
            .SetUrl(
                "~/OrchardCore.Commerce/css/shopping-cart.min.css",
                "~/OrchardCore.Commerce/css/shopping-cart.css")
            .SetVersion("1.0.0");

        _manifest
            .DefineStyle(ShoppingCartWidget)
            .SetUrl(
                "~/OrchardCore.Commerce/css/shopping-cart-widget.min.css",
                "~/OrchardCore.Commerce/css/shopping-cart-widget.css")
            .SetVersion("1.0.0");

        _manifest
            .DefineScript(CommerceRegions)
            .SetDependencies(JQuery)
            .SetUrl(
                "~/OrchardCore.Commerce/js/commerce-regions.min.js",
                "~/OrchardCore.Commerce/js/commerce-regions.js")
            .SetVersion("1.0.0");

        _manifest
            .DefineScript(StripeCardForm)
            .SetUrl(
                "~/OrchardCore.Commerce/js/stripe-card-form.min.js",
                "~/OrchardCore.Commerce/js/stripe-card-form.js")
            .SetVersion("1.0.0");
    }

    public void Configure(ResourceManagementOptions options) => options.ResourceManifests.Add(_manifest);
}
