using Lombiq.Tests.UI.SecurityScanning;
using Microsoft.CodeAnalysis.Sarif;
using Shouldly;
using System.Text.Json;
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
                    configuration.DisableActiveScanRule(40024, "SQL Injection - SQLite (everything goes through YesSql so these are false positive)");
                    FalsePositive(
                        configuration,
                        10202,
                        "Absence of Anti-CSRF Tokens",
                        "The ProductListPart-Filters intentionally uses a GET form. No XSS risk.",
                        @"https://[^/]+/",
                        @".*/\?.*pagenum=.*",
                        @".*/\?.*products\..*");
                },
                sarifLog =>
                {
                    var errors = sarifLog
                        .Runs[0]
                        .Results
                        .Where(result =>
                            result.Kind == ResultKind.Fail &&
                            result.Level != FailureLevel.None &&
                            result.Level != FailureLevel.Note &&
                            // Exclude the specific false positive that was already excluded above in the configuration.
                            // See https://github.com/Lombiq/UI-Testing-Toolbox/issues/336 for more details.
                            result.Locations?.Any(location =>
                                location.PhysicalLocation?.Region?.Snippet?.Text == "<form method=\"get\" action=\"/\">") != true)
                        .Select(result => new
                        {
                            Kind = result.Kind.ToString(),
                            Level = result.Level.ToString(),
                            Details = result,
                        })
                        .ToList();
                    errors.ShouldBeEmpty(JsonSerializer.Serialize(errors));
                }));

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
