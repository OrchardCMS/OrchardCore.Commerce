using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using SampleWebApp.Tests.UI.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace SampleWebApp.Tests.UI.Tests.BasicOrchardFeaturesTests;

public class BasicOrchardFeaturesTests : UITestBase
{
    public BasicOrchardFeaturesTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Theory, Chrome]
    public Task AdminDashboardShouldBeAccessible(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                await context.SignInDirectlyAsync();
                await context.GoToDashboardAsync();

                context.Exists(By.ClassName("alert-success"));
            },
            browser);

    [Theory, Chrome]
    public Task BasicOrchardFeaturesShouldWork(Browser browser) =>
        ExecuteTestAsync(
            context => context.TestBasicOrchardFeaturesExceptRegistrationAsync(SetupHelpers.RecipeId),
            browser,
            configuration =>
            {
                configuration.AccessibilityCheckingConfiguration.RunAccessibilityCheckingAssertionOnAllPageChanges = true;
                configuration.AccessibilityCheckingConfiguration.AxeBuilderConfigurator += axeBuilder =>
                    AccessibilityCheckingConfiguration
                        .ConfigureWcag21aa(axeBuilder)
                        .DisableRules("color-contrast", "link-name");

                return Task.CompletedTask;
            });
}
