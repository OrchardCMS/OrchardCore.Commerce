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
        ExecuteTestAfterSetupAsync(context =>
            context.RunAndConfigureAndAssertFullSecurityScanForAutomationAsync(ConfigureSecurityScan));

    private static void ConfigureSecurityScan(SecurityScanConfiguration configuration)
    {
        // Not sure if we need anything here, for now.
    }
}
