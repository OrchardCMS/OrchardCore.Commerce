using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Models;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;

namespace OrchardCore.Commerce.Tests.UI.Helpers;

public static class SetupHelpers
{
    public const string RecipeId = "OrchardCore.Commerce.Development.Tests.Setup";

    public static async Task<Uri> RunSetupAsync(UITestContext context)
    {
        var homepageUri = await context.GoToSetupAndSetupOrchardCoreAsync(
            new OrchardCoreSetupParameters(context)
            {
                SiteName = "Orchard Setup",
                RecipeId = RecipeId,
                SiteTimeZoneValue = "Europe/London",
            });

        context.Exists(By.Id("mainNav"));

        return homepageUri;
    }
}
