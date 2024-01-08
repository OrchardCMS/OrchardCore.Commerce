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
            FalsePositive(
                configuration,
                10202,
                "Absence of Anti-CSRF Tokens: The ProductListPart-Filters intentionally uses a GET form. No XSS risk.",
                @"https://[^/]+/",
                @".*/\?.*pagenum=.*", // #spell-check-ignore-line
                @".*/\?.*products\..*")));

    private static void FalsePositive(
        SecurityScanConfiguration configuration,
        int id,
        string justification,
        params string[] urls)
    {
        foreach (var url in urls)
        {
            configuration.MarkScanRuleAsFalsePositiveForUrlWithRegex(url, id, justification);
        }
    }
}
