using Lombiq.Tests.UI.SecurityScanning;
using Microsoft.CodeAnalysis.Sarif;
using Newtonsoft.Json;
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
            context => context.RunAndConfigureAndAssertFullSecurityScanForAutomationAsync(
                configuration => FalsePositive(
                    configuration,
                    10202,
                    "Absence of Anti-CSRF Tokens: The ProductListPart-Filters intentionally uses a GET form. No XSS risk.",
                    @"https://[^/]+/",
                    @".*/\?.*pagenum=.*", // #spell-check-ignore-line
                    @".*/\?.*products\..*"),
                sarifLog =>
                {
                    var errors = sarifLog
                        .Runs[0]
                        .Results
                        .Where(result =>
                            result.Kind == ResultKind.Fail &&
                            result.Level != FailureLevel.None &&
                            result.Level != FailureLevel.Note &&
                            // Exclude this specific false positive that was mentioned above in the configuration. It
                            // somehow shows up in the report JSON, but not in the report HTML, suggesting that this is
                            // some kind of a bug with ZAP.
                            result
                                .Locations?
                                .Any(location => location.PhysicalLocation?.Region?.Snippet?.Text == "<form method=\"get\" action=\"/\">") != true)
                        .Select(result => new
                        {
                            Kind = result.Kind.ToString(),
                            Level = result.Level.ToString(),
                            Details = result,
                        })
                        .ToList();
                    errors.ShouldBeEmpty(JsonConvert.SerializeObject(errors));
                }));

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
