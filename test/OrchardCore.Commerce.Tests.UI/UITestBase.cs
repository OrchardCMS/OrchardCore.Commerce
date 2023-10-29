using Atata.HtmlValidation;
using Lombiq.Tests.UI;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OrchardCore.Commerce.Tests.UI.Helpers;
using Shouldly;
using Xunit.Abstractions;

namespace OrchardCore.Commerce.Tests.UI;

public class UITestBase : OrchardCoreUITestBase<Program>
{
    protected UITestBase(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    protected override Task ExecuteTestAfterSetupAsync(
        Func<UITestContext, Task> testAsync,
        Browser browser,
        Func<OrchardCoreUITestExecutorConfiguration, Task> changeConfigurationAsync) =>
        ExecuteTestAsync(testAsync, browser, SetupHelpers.RunSetupAsync, changeConfigurationAsync);

    protected override Task ExecuteTestAsync(
        Func<UITestContext, Task> testAsync,
        Browser browser,
        Func<UITestContext, Task<Uri>> setupOperation,
        Func<OrchardCoreUITestExecutorConfiguration, Task> changeConfigurationAsync) =>
        base.ExecuteTestAsync(
            testAsync,
            browser,
            setupOperation,
            async configuration =>
            {
                configuration.AccessibilityCheckingConfiguration.RunAccessibilityCheckingAssertionOnAllPageChanges = true;
                configuration.AccessibilityCheckingConfiguration.AxeBuilderConfigurator += axeBuilder =>
                    AccessibilityCheckingConfiguration
                        .ConfigureWcag21aa(axeBuilder)
                        .DisableRules("color-contrast");
                configuration.HtmlValidationConfiguration.AssertHtmlValidationResultAsync =
                    AssertHtmValidationResultAsync;

                if (changeConfigurationAsync != null) await changeConfigurationAsync(configuration);
            });

    private static async Task AssertHtmValidationResultAsync(HtmlValidationResult validationResult)
    {
        // The built-in Orchard Core pager has a rel attribute on the <a> tag which is not allowed by the HTML
        // specification. We are ignoring this error for all tests, since most of them will display the pager.
        var errors = (await validationResult.GetErrorsAsync())
            .Where(error => !error.ContainsOrdinalIgnoreCase("\"rel\" attribute cannot be used in this context"));
        errors.ShouldBeEmpty();
    }
}
