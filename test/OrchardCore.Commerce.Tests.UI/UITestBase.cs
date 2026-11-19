using Lombiq.Tests.UI;
using Lombiq.Tests.UI.Services;
using OrchardCore.Commerce.Tests.UI.Helpers;
using Xunit;

namespace OrchardCore.Commerce.Tests.UI;

public class UITestBase : OrchardCoreUITestBase<Program>
{
    protected UITestBase(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        UseSetup(SetupHelpers.RunSetupAsync);

        AddConfiguration(configuration =>
        {
            configuration.AccessibilityCheckingConfiguration.RunAccessibilityCheckingAssertionOnAllPageChanges = true;
            configuration.AccessibilityCheckingConfiguration.AxeBuilderConfigurator += axeBuilder =>
                AccessibilityCheckingConfiguration
                    .ConfigureWcag21aa(axeBuilder)
                    .DisableRules("color-contrast");
        });
    }
}
