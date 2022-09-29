using Atata;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OrchardCore.Commerce.Tests.UI.Tests.PersistenceRegionSettingsTests;

public class PersistenceRegionSettingsTests : UITestBase
{
    public PersistenceRegionSettingsTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Theory, Chrome]
    public Task RegionSettingsShouldPersist(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                await context.SignInDirectlyAsync();
                await context.ExecuteRecipeDirectlyAsync("RegionTest");

                await context.CreateNewContentItemAsync("Order");

                var selectListOptions = context
                    .GetAll(By.XPath("//select[@id='OrderPart_BillingAddress_Address_Region']/option"));

                selectListOptions.Count.ShouldBe(3);

                var selectListTexts = selectListOptions.Select(selectListOption => selectListOption.Text).AsList();

                selectListTexts[0].ShouldBe("Argentina");
                selectListTexts[1].ShouldBe("Luxemburg");
                selectListTexts[2].ShouldBe("Magyarország");
            },
            browser);
}
