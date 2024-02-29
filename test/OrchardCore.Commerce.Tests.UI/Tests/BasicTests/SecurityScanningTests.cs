using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.SecurityScanning;
using Shouldly;
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
                }),
            changeConfiguration: configuration => configuration.AssertAppLogsAsync = async webApplicationInstance =>
            {
                var logsWithoutUnwantedExceptionMessages = (await webApplicationInstance.GetLogOutputAsync())
                    .SplitByNewLines()
                    .Where(message =>
                        !message.ContainsOrdinalIgnoreCase("System.IO.DirectoryNotFoundException: Could not find a part of the path") &&
                        !message.ContainsOrdinalIgnoreCase(
                            "System.IO.IOException: The filename, directory name, or volume label syntax is incorrect") &&
                        !message.ContainsOrdinalIgnoreCase("System.InvalidOperationException: This action intentionally causes an exception!"));

                logsWithoutUnwantedExceptionMessages.ShouldNotContain(item => item.Contains("|ERROR|"));
            });

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
