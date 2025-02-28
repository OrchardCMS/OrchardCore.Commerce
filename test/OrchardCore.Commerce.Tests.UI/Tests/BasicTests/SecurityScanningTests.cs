using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Helpers;
using Lombiq.Tests.UI.SecurityScanning;
using Lombiq.Tests.UI.Services;
using Lombiq.Tests.UI.Shortcuts.Controllers;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
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

                    // More detailed error reporting.
                    ConfigureAppLogAssertion(context.Configuration);
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

    [SuppressMessage(
        "StyleCop.CSharp.ReadabilityRules",
        "SA1114:Parameter list should follow declaration",
        Justification = "Disregarded to improve commenting readability.")]
    private static void ConfigureAppLogAssertion(OrchardCoreUITestExecutorConfiguration configuration)
    {

        // Copied from OrchardCoreUITestExecutorConfigurationExtensions.
        var permittedErrorLinePatterns = new List<string>
        {
            // The model binding will throw FormatException exception with this text during ZAP active scan, when the
            // bot tries to send malicious query strings or POST data that doesn't fit the types expected by the model.
            // This is correct, safe behavior and should be logged in production.
            "is not a valid value for Boolean",
            "An unhandled exception has occurred while executing the request. System.FormatException: any",
            "System.FormatException: The input string '[\\S\\s]+' was not in a correct format.",
            "System.FormatException: The input string 'any",
            // Happens when the static file middleware tries to access a path that doesn't exist or access a file as a
            // directory. Presumably this is an attempt to access protected files using source path manipulation. This
            // is handled by ASP.NET Core and there is nothing for us to worry about.
            "System.IO.IOException: Not a directory",
            "System.IO.IOException: The filename, directory name, or volume label syntax is incorrect",
            "System.IO.DirectoryNotFoundException: Could not find a part of the path",
            // This happens when a request's model contains a dictionary and a key is missing. While this can be a
            // legitimate application error, during a security scan it's more likely the result of an incomplete
            // artificially constructed request. So the means the ASP.NET Core model binding is working as intended.
            "An unhandled exception has occurred while executing the request. System.ArgumentNullException: " +
            "Value cannot be null. (Parameter 'key')",
            // One way to verify correct error handling is to navigate to ~/Lombiq.Tests.UI.Shortcuts/Error/Index, which
            // always throws an exception. This also gets logged but it's expected, so it should be ignored.
            ErrorController.ExceptionMessage,
            // Thrown from Microsoft.AspNetCore.Authentication.AuthenticationService.ChallengeAsync() when ZAP sends
            // invalid authentication challenges.
            "System.InvalidOperationException: No authentication handler is registered for the scheme",
            // If the middleware is enabled, logs like this are emitted next to every exception even if they are
            // already suppressed by one of these patterns.
            "Lombiq.Tests.UI.Shortcuts.Middlewares.ExceptionContextLoggingMiddleware: HTTP request when the exception",
        };

        // Custom values.
        permittedErrorLinePatterns.AddRange([
            // Happens occasionally when the active scan submits invalid data.
            "System.ArgumentNullException: Value cannot be null. (Parameter 'key')"
        ]);

        configuration.AssertAppLogsAsync = app =>
            app.LogsShouldNotContainAsync(logEntry =>
                logEntry.Level >= LogLevel.Error &&
                AppLogAssertionHelper.NotMediaCacheEntries(logEntry) &&
                !permittedErrorLinePatterns.Any(pattern =>
                    logEntry.ToString().RegexIsMatch(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled, null)));
    }

    private static async Task AssertLogsAsync(
        IWebApplicationInstance webApplicationInstance,
        Expression<Func<IApplicationLogEntry, bool>> logEntryPredicate,
        Action<IEnumerable<IApplicationLogEntry>, Expression<Func<IApplicationLogEntry, bool>>, string> shouldlyMethod, // #spell-check-ignore-line
        CancellationToken cancellationToken = default)
    {
        var logs = (await webApplicationInstance.GetLogsAsync(cancellationToken))
            .ToList();
        var logContents = await logs.ToFormattedStringAsync();

        foreach (var log in logs)
        {
            shouldlyMethod(await log.GetEntriesAsync(), logEntryPredicate, logContents); // #spell-check-ignore-line
        }
    }
}
