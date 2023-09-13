using Atata;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using static OrchardCore.Commerce.Tests.UI.Constants.ContentItemIds;

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
                await context.GoToContentItemByIdAsync(TestProduct);

                // Also testing shopping cart widget.
                ShoppingCartItemCountShouldBe(context, 0);

                await context.ClickReliablyOnSubmitAsync();

                ShoppingCartItemCountShouldBe(context, 1);

                context.Get(By.ClassName("shopping-cart-widget")).Click();

                context.Driver.Exists(By.XPath($"//a[contains(., 'Test Product')]").Visible());
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

            context.Driver.Exists(By.XPath($"//li[contains(., 'PriceVariantsProduct: Small')]").Visible());
        },
        browser);

    [Theory, Chrome]
    public Task PriceEstimationWithMinimumOrderQuantityShouldNotShowWarning(Browser browser) =>
        ExecuteTestAfterSetupAsync(
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
            },
            browser);

    private static void ShoppingCartItemCountShouldBe(UITestContext context, int count) =>
        context.Get(By.ClassName("shopping-cart-item-count")).Text.ShouldBeAsString(count);
}
