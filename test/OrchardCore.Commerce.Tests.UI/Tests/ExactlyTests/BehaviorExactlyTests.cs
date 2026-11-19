using Lombiq.Tests.UI.Extensions;
using OpenQA.Selenium;
using OrchardCore.Commerce.Payment.Exactly.Constants;
using OrchardCore.Commerce.Payment.Exactly.Drivers;
using Shouldly;
using Xunit;

namespace OrchardCore.Commerce.Tests.UI.Tests.ExactlyTests;

public class BehaviorExactlyTests : UITestBase
{
    public BehaviorExactlyTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Fact]
    public Task ExactlySettingsSignUpLinkShouldDisplayCorrectly() =>
        RunAsync(
            async context =>
            {
                await context.SignInDirectlyAsync();
                await context.EnableFeatureDirectlyAsync(FeatureIds.Area);

                await context.GoToAdminRelativeUrlAsync("/Settings/Exactly");
                context
                    .Get(By.CssSelector(".exactly-sign-up-info a"))
                    .GetAttribute("href")
                    .ShouldBe(ExactlySettingsDisplayDriver.SignUpLink);
            });
}
