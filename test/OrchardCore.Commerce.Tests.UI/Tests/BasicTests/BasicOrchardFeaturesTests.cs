using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.BasicOrchardFeaturesTesting;
using Lombiq.Tests.UI.Services;
using OrchardCore.Commerce.Tests.UI.Helpers;
using Xunit;

namespace OrchardCore.Commerce.Tests.UI.Tests.BasicTests;

public class BasicOrchardFeaturesTests : UITestBase
{
    public BasicOrchardFeaturesTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Theory, Chrome]
    public Task BasicOrchardFeaturesShouldWork(Browser browser) =>
        ExecuteTestAsync(
            context => context.TestBasicOrchardFeaturesExceptRegistrationAsync(SetupHelpers.RecipeId),
            browser);
}
