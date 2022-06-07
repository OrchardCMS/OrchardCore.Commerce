using Lombiq.Tests.UI;
using Lombiq.Tests.UI.Helpers;
using Lombiq.Tests.UI.Services;
using OrchardCore.Commerce.Tests.UI.Helpers;
using Xunit.Abstractions;

namespace OrchardCore.Commerce.Tests.UI;

public class UITestBase : OrchardCoreUITestBase
{
    protected override string AppAssemblyPath => WebAppConfigHelper
        .GetAbsoluteApplicationAssemblyPath("OrchardCore.Commerce.Web", "net6.0");

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
        Action<UITestContext> test,
        Browser browser,
        Func<UITestContext, Task<Uri>>? setupOperation = null,
        Action<OrchardCoreUITestExecutorConfiguration>? changeConfiguration = null) =>
        base.ExecuteTestAsync(
            test,
            browser,
            setupOperation,
            configuration =>
            {
                configuration.AccessibilityCheckingConfiguration.RunAccessibilityCheckingAssertionOnAllPageChanges = true;
                configuration.UseSqlServer = true;

                changeConfiguration?.Invoke(configuration);
            });
}
