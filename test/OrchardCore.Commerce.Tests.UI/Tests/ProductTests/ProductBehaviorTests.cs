using Atata;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
using Xunit;
using static OrchardCore.Commerce.Tests.UI.Constants.ContentItemIds;

namespace OrchardCore.Commerce.Tests.UI.Tests.ProductTests;

public class ProductBehaviorTests : UITestBase
{
    public ProductBehaviorTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Fact]
    public Task ProductCanBeAddedToShoppingCart() =>
        RunAsync(
            async context =>
            {
                await context.SignInDirectlyAsync();
                await context.GoToContentItemByIdAsync(TestProduct);

                // Also testing shopping cart widget.
                ShoppingCartItemCountShouldBe(context, 0);

                await context.ClickReliablyOnSubmitAsync();

                ShoppingCartItemCountShouldBe(context, 1);

                context.Get(By.ClassName("shopping-cart-widget")).Click();

                context.Driver.Exists(By.XPath("//a[contains(., 'Test Product')]").Visible());
            });

    [Fact]
    public Task PriceVariantsProductCanBeAddedToShoppingCart() =>
    RunAsync(
        async context =>
        {
            await context.SignInDirectlyAsync();
            await context.GoToContentItemByIdAsync("testpricevariantproduct000");

            await context.ClickReliablyOnSubmitAsync();

            context.Driver.Exists(By.XPath("//li[contains(., 'Size: Small')]").Visible());
        });

    [Fact]
    public Task PriceEstimationWithMinimumOrderQuantityShouldNotShowWarning() =>
        RunAsync(
            async context =>
            {
                await context.SignInDirectlyAsync();
                await context.GoToContentItemEditorByIdAsync(TestProduct);

                await context.ClickAndFillInWithRetriesAsync(By.Id("InventoryPart_MinimumOrderQuantity_Value"), "2");
                await context.ClickAndFillInWithRetriesAsync(By.Id("InventoryPart_MaximumOrderQuantity_Value"), "5");

                await context.ClickReliablyOnSubmitAsync();
                context.ShouldBeSuccess();

                await context.GoToContentItemByIdAsync(TestProduct);
                context.ErrorMessageShouldNotExist();
            });

    [Fact]
    public Task ShoppingCartWidgetShouldCountCorrectly() =>
        RunAsync(
            async context =>
            {
                void ShoppingCartShouldBe(int count) =>
                    context.Get(By.ClassName("shopping-cart-item-count")).Text.Trim().ShouldBeAsString(count);

                await context.SignInDirectlyAndGoToHomepageAsync();
                ShoppingCartShouldBe(0);

                await context.GoToContentItemByIdAsync(TestFreeProduct);
                await context.ClickReliablyOnSubmitAsync();
                ShoppingCartShouldBe(1);

                await context.ClickAndFillInWithRetriesAsync(By.Name("cart.lines[0].Quantity"), "2");
                await context.ClickReliablyOnAsync(By.CssSelector("#shopping-cart-update button"));
                ShoppingCartShouldBe(2);
            });

    private static void ShoppingCartItemCountShouldBe(UITestContext context, int count) =>
        context.Get(By.ClassName("shopping-cart-item-count")).Text.ShouldBeAsString(count);
}
