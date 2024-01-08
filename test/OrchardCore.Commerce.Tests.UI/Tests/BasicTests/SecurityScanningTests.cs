using Lombiq.Tests.UI.SecurityScanning;
using Xunit;
using Xunit.Abstractions;

namespace OrchardCore.Commerce.Tests.UI.Tests.BasicTests;

public class SecurityScanningTests : UITestBase
{
    public SecurityScanningTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Fact]
    public Task FullSecurityScanShouldPass() =>
        ExecuteTestAfterSetupAsync(context => context.RunAndConfigureAndAssertFullSecurityScanForAutomationAsync(configuration =>
            configuration.MarkScanRuleAsFalsePositiveForUrlWithRegex(
                @"https://[^/]+/(\?.*pagenum=[0-9]+&.*)?(\?.*products\..*)?",
                10202,
                "Absence of Anti-CSRF Tokens: The product list filters are intentionally in a GET form. No XSS risk here.")));
}
