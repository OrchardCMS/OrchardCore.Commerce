using Lombiq.Tests.UI;
using Lombiq.Tests.UI.Services;
using OrchardCore.Commerce.Tests.UI.Helpers;
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
                configuration.AssertBrowserLog = logEntries =>
                {
                    // This is to not fail on a browser error caused by jQuery missing. Can be removed after this issue is
                    // resolved and released: https://github.com/OrchardCMS/OrchardCore/issues/15181.
                    var messageWithoutJqueryError = logEntries.Where(logEntry =>
                        !logEntry.Message.ContainsOrdinalIgnoreCase(
                            "Uncaught ReferenceError: $ is not defined"));

                    OrchardCoreUITestExecutorConfiguration.AssertBrowserLogIsEmpty(messageWithoutJqueryError);
                };

                configuration.HtmlValidationConfiguration.HtmlValidationOptions =
                    configuration.HtmlValidationConfiguration.HtmlValidationOptions
                        .CloneWith(validationOptions => validationOptions.ConfigPath =
                            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "orchardcore.htmlvalidate.json"));

                if (changeConfigurationAsync != null) await changeConfigurationAsync(configuration);
            });
}
