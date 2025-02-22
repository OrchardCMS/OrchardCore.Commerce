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
        ExecuteTestAfterSetupAsync(
            context => context.RunAndConfigureAndAssertFullSecurityScanForContinuousIntegrationAsync(
                configuration =>
                {
                    configuration.DisableActiveScanRule(
                        6,
                        "Path Traversal (all paths are virtual so it's not a real concern, also creates too many errors)");

                    configuration.DisableActiveScanRule(
                        40024,
                        "SQL Injection - SQLite (everything goes through YesSql so these are false positive)");

                    configuration.DisableActiveScanRule(
                        40027,
                        "The query time is controllable using parameter value [some SQL injection]");

                    FalsePositive(
                        configuration,
                        10202,
                        "Absence of Anti-CSRF Tokens",
                        "The ProductListPart-Filters intentionally uses a GET form. No XSS risk.",
                        @"https://[^/]+/",
                        @".*/\?.*pagenum=.*",
                        @".*/\?.*products\..*");
                },
                maxActiveScanDurationInMinutes: 5,
                maxRuleDurationInMinutes: 1,
                additionalPermittedErrorLinePatterns:
                [
                    // Happens occasionally when the active scan submits invalid data.
                    "System.ArgumentNullException: Value cannot be null. (Parameter 'key')"
                ]));

    private static void FalsePositive(
        SecurityScanConfiguration configuration,
        int id,
        string name,
        string justification,
        params string[] urls)
    {
        foreach (var url in urls)
        {
            configuration.MarkScanRuleAsFalsePositiveForUrlWithRegex(url, id, name, justification);
        }
    }
}
