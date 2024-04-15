using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using OrchardCore.Commerce.Payment.Exactly.Constants;
using OrchardCore.Commerce.Payment.Exactly.Drivers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OrchardCore.Commerce.Tests.UI.Tests.ExactlyTests;

public class BehaviorExactlyTests : UITestBase
{
    public BehaviorExactlyTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Theory, Chrome]
    public Task ExactlySettingsSignupLinkShouldDisplayCorrectly(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                await context.SignInDirectlyAsync();
                await context.EnableFeatureDirectlyAsync(FeatureIds.Area);

                await context.GoToAdminRelativeUrlAsync("/Settings/Exactly");
                context
                    .Get(By.CssSelector(".exactly-signup-info a"))
                    .GetAttribute("href")
                    .ShouldBe(ExactlySettingsDisplayDriver.SignupLink);
            },
            browser);
}
