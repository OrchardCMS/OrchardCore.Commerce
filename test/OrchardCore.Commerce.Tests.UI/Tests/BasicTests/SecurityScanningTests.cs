using Lombiq.Tests.UI.SecurityScanning;
using Xunit;

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

                    configuration.DisableSubresourceIntegrityAttributeMissingRuleForGoogleFonts();

                    FalsePositive(
                        configuration,
                        40018,
                        "SQL Injection",
                        "It says \"The page results were successfully manipulated using the boolean conditions\", " +
                        "but the \"manipulation\" is simply that the site returns the error screen \"Your browser " +
                        "sent a request that this server could not understand.\" with an error code, so the problem " +
                        "is already handled internally by OC.",
                        @".*/shoppingcart/AddItem.*");

                    FalsePositive(
                        configuration,
                        10202,
                        "Absence of Anti-CSRF Tokens",
                        "The ProductListPart-Filters intentionally uses a GET form. No XSS risk.",
                        @"https://[^/]+/",
                        @".*/\?.*pagenum=.*",
                        @".*/\?.*products\..*");

                    // Not relevant for testing OCC.
                    configuration.DontScanErrorPage = true;
                },
                maxActiveScanDurationInMinutes: 5,
                maxRuleDurationInMinutes: 1));

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
