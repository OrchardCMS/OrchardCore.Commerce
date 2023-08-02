using Atata;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
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
                    context.CheckExistence(By.CssSelector(".price-part-price-field-value del"), hasNewPrice);
                }

                await context.ExecuteRecipeDirectlyAsync("OrchardCore.Commerce.Samples.GlobalDiscount");

                await CheckDiscountPresenceAsync(hasNewPrice: false);
                await context.SignInDirectlyAsync();
                await CheckDiscountPresenceAsync(hasNewPrice: true);
            },
            browser);

    [Theory, Chrome]
    public Task DiscountShouldProperlyAppearOnDiscountedProduct(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                // Discount should appear on discounted product's page.
                await context.GoToRelativeUrlAsync("/testdiscountedproduct");
                context.Get(By.CssSelector(".price-part-price-field-value > del")).Text.ShouldBe("$5.00");
                context.Get(By.CssSelector(".price-part-price-field-value > span")).Text.ShouldBe("$3.00");
                await context.ClickReliablyOnSubmitAsync();

                // Discount should appear in cart when discounted product is the only item in the cart.
                context.Get(By.CssSelector(".shopping-cart-table-unit-price")).Text.ShouldBe("$3.00");

                // Discount should appear in cart when discounted product is not the only item in the cart.
                await context.GoToRelativeUrlAsync("/testproduct");
                await context.ClickReliablyOnSubmitAsync();
                context.Get(By.CssSelector(".shopping-cart-table-unit-price")).Text.ShouldBe("$3.00");

                // Total should reflect discount as well.
                context.Get(By.CssSelector(".shopping-cart-table-totals > div")).Text.ShouldBe("$8.00");

                // Discount should still appear on discounted product's page when the cart contains a full-price item.
                await context.GoToRelativeUrlAsync("/testdiscountedproduct");
                context.Get(By.CssSelector(".price-part-price-field-value > del")).Text.ShouldBe("$5.00");
                context.Get(By.CssSelector(".price-part-price-field-value > span")).Text.ShouldBe("$3.00");
            },
            browser);
}
