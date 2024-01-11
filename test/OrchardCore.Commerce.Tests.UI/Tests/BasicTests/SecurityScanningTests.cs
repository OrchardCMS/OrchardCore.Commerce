using Atata.WebDriverSetup;
using Lombiq.Tests.UI.SecurityScanning;
using Lombiq.Tests.UI.Services.GitHub;
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
        // On GitHub when running from Windows, this test always fails with the following error:
        // The `docker.exe pull softwaresecurityproject/zap-stable:2.14.0 --quiet` command failed with the output below.
        // no matching manifest for windows/amd64 10.0.20348 in the manifest list entries
        // This is because the Docker on the runner is in Windows mode but zap-stable only has Linux images.
        GitHubHelper.IsGitHubEnvironment && OSInfo.IsWindows
            ? Task.CompletedTask
            : ExecuteTestAfterSetupAsync(
                context => context.RunAndConfigureAndAssertFullSecurityScanForAutomationAsync(
                    configuration => FalsePositive(
                        configuration,
                        10202,
                        "Absence of Anti-CSRF Tokens: The ProductListPart-Filters intentionally uses a GET form. No XSS risk.",
                        @"https://[^/]+/",
                        @".*/\?.*pagenum=.*",
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
