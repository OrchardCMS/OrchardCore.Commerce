using Lombiq.Tests.UI;
using Lombiq.Tests.UI.Helpers;
using Lombiq.Tests.UI.Services;
using SampleWebApp.Tests.UI.Helpers;
using System.Drawing;
using Xunit.Abstractions;

namespace SampleWebApp.Tests.UI;

public class UITestBase : OrchardCoreUITestBase
{
    protected override string AppAssemblyPath => WebAppConfigHelper
        .GetAbsoluteApplicationAssemblyPath("SampleWebApp", "net6.0");

    protected override Size MobileBrowserSize { get; } = new(414, 736);

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
        Func<UITestContext, Task<Uri>> setupOperation = null,
        Action<OrchardCoreUITestExecutorConfiguration> changeConfiguration = null) =>
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
