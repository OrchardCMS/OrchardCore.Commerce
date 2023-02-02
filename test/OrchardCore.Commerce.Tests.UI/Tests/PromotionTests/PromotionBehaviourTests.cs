using Atata;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Xunit;
using Xunit.Abstractions;
using static OrchardCore.Commerce.Tests.UI.Constants.ContentItemIds;

namespace OrchardCore.Commerce.Tests.UI.Tests.PromotionTests;

public class PromotionBehaviourTests : UITestBase
{
    public PromotionBehaviourTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Theory, Chrome]
    public Task GlobalDiscountShouldOnlyAppearAfterAuthenticated(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                async Task CheckDiscountPresenceAsync(bool hasNewPrice)
                {
                    await context.GoToContentItemByIdAsync(TestProduct);
                    context.Exists(By.ClassName("price-part-price-field-value"));
                    context.CheckExistence(By.ClassName("field-name-discount-part-new-price"), hasNewPrice);
                }

                await context.ExecuteRecipeDirectlyAsync("OrchardCore.Commerce.Samples.GlobalDiscount");

                await CheckDiscountPresenceAsync(hasNewPrice: false);
                await context.SignInDirectlyAsync();
                await CheckDiscountPresenceAsync(hasNewPrice: true);
            },
            browser);
}
