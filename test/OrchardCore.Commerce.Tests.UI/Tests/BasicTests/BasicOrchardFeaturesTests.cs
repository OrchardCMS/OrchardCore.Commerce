using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.BasicOrchardFeaturesTesting;
using Lombiq.Tests.UI.Models;
using Lombiq.Tests.UI.Services;
using OrchardCore.Commerce.Tests.UI.Helpers;
using Xunit;

namespace OrchardCore.Commerce.Tests.UI.Tests.BasicTests;

public class BasicOrchardFeaturesTests : UITestBase
{
    private static readonly OrchardCoreSetupParameters _setupParameters = new()
    {
        RecipeId = SetupHelpers.RecipeId,
        SkipRegistration = true,
    };

    public BasicOrchardFeaturesTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Fact]
    public Task BasicOrchardFeaturesShouldWork() =>
        ExecuteTestAsync(context => context.TestBasicOrchardFeaturesAsync(_setupParameters));
}
