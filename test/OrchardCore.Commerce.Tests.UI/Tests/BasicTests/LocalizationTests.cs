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

                context
                    .Get(By.CssSelector(cssTranslatedTextElement))
                    .GetTextTrimmed()
                    .ShouldBe("Kedvezményes ár adóval:");
            },
            browser);
}
