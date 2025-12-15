using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
using Xunit;

namespace OrchardCore.Commerce.Tests.UI.Tests.BasicTests;

public class LocalizationTests : UITestBase
{
    public LocalizationTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Theory, Chrome]
    public Task NuGetPackageWithLocalizationShouldApplyTranslatedText(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                const string cssTranslatedTextElement =
                    ".content-product:has(a[href*='/testdiscountedproduct']) .field-name-discount-part-new-price strong";

                // Switch to another language (Hungarian) in the admin.
                await context.SignInDirectlyAndGoToAdminRelativeUrlAsync("/Settings/localization");
                await context.ClickReliablyOnAsync(By.XPath("//tr[td[1]/span[contains(., 'en-US')]]//a[@title='Remove culture']"));
                await context.ClickReliablyOnAsync(By.ClassName("save"));

                // Verify that localized text is visible on the home page.
                await context.GoToHomePageAsync();
                context
                    .Get(By.CssSelector(cssTranslatedTextElement))
                    .GetTextTrimmed()
                    .ShouldBe("Kedvezményes ár adóval:");
            },
            browser);
}
