using Atata;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OrchardCore.Commerce.Tests.UI.Tests.ProductTests;

public class ProductBehaviourTests : UITestBase
{
    public ProductBehaviourTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Theory, Chrome]
    public Task ProductCanBeAddedToShoppingCart(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                await context.SignInDirectlyAsync();
                await context.GoToContentItemByIdAsync("testproduct000");

                // Also testing shopping cart widget.
                ShoppingCartItemCountShouldBe(context, 0);

                await context.ClickReliablyOnSubmitAsync();

                ShoppingCartItemCountShouldBe(context, 1);

                context.Get(By.ClassName("widget-shopping-cart-widget")).Click();

                context.Driver.Exists(By.XPath($"//a[contains(text(), 'TestProduct')]").Visible());
            },
            browser);

    [Theory, Chrome]
    public Task PriceVariantsProductCanBeAddedToShoppingCart(Browser browser) =>
    ExecuteTestAfterSetupAsync(
        async context =>
        {
            await context.SignInDirectlyAsync();
            await context.GoToContentItemByIdAsync("testpricevariantproduct000");

            await context.ClickReliablyOnSubmitAsync();

            context.Driver.Exists(By.XPath($"//li[contains(text(), 'PriceVariantsProduct: Small')]").Visible());
        },
        browser);

    private static void ShoppingCartItemCountShouldBe(UITestContext context, int count) =>
        context.Get(By.ClassName("item-count")).Text.ShouldBeAsString(count);
}
