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
        {
            const string antiCsrfJustification =
                "Absence of Anti-CSRF Tokens: The ProductListPart-Filters intentionally uses a GET form. No XSS risk.";

            configuration.MarkScanRuleAsFalsePositiveForUrlWithRegex(@"https://[^/]+/", 10202, antiCsrfJustification);
            configuration.MarkScanRuleAsFalsePositiveForUrlWithRegex(@".*/\?.*pagenum=.*", 10202, antiCsrfJustification);
            configuration.MarkScanRuleAsFalsePositiveForUrlWithRegex(@".*/\?.*products\..*", 10202, antiCsrfJustification);
        }));
}
