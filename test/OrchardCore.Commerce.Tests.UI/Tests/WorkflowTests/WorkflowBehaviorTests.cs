using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using OrchardCore.Commerce.Tests.UI.Constants;
using Shouldly;
using Xunit;

namespace OrchardCore.Commerce.Tests.UI.Tests.WorkflowTests;

public class WorkflowBehaviorTests : UITestBase
{
    public WorkflowBehaviorTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Fact]
    public Task CartVerifyingItemEventShouldDisplayError() =>
        
        RunAfterCartWorkflowsRecipe(
            async context =>
            {
                // Add only the free product to the cart. This is blocked by the "Item Verification Sample" workflow.
                await context.GoToContentItemDisplayByIdAsync(ContentItemIds.TestFreeProduct);
                await context.ClickReliablyOnSubmitAsync();

                // Due to the expected verification failure, the cart should still be empty and the error message shown.
                context.Driver.Url.ShouldEndWith("/cart/empty");
                context
                    .GetErrorMessage()
                    .ShouldBe("The \"Item Verification Sample\" workflow has intentionally failed this product.");
            });

    [Fact]
    public Task CartEventsShouldUpdateTableAndAddLineItem() =>
        RunAfterCartWorkflowsRecipe(
            async context =>
            {
                // Add only the test product to the cart.
                await context.GoToContentItemDisplayByIdAsync(ContentItemIds.TestProduct);
                await context.ClickReliablyOnSubmitAsync();

                // Verify that the additional product is added and so the price is higher.
                const string price = "$10.00";
                context.Get(By.ClassName("shopping-cart-table-totals")).Text.Trim().ShouldBe(price);

                // Verify that the row count is 4 (column headers, 2 line items and summary row), the additional column
                // is added, and its contents are as expected.
                var lastColumn = context.GetAll(By.CssSelector(".shopping-cart-table tr > *:last-child"));
                lastColumn.Count.ShouldBe(4);
                lastColumn
                    .Take(3)
                    .Select(element => element.Text.Trim())
                    .ToArray()
                    .ShouldBe(
                    [
                        "Workflow",
                        "Some content about product \"testproduct000\".",
                        "Some content about product \"shipping000000000000000000\".",
                    ]);

                // Verify that it still works even after a reload.
                await context.RefreshAsync();
                context.Get(By.ClassName("shopping-cart-table-totals")).Text.Trim().ShouldBe(price);
            });

    private Task RunAfterCartWorkflowsRecipe(Func<UITestContext, Task> test) => TestBuilder
        .AddTest(async context =>
        {
            await context.SignInDirectlyAsync();
            await context.ExecuteRecipeDirectlyAsync("OrchardCore.Commerce.Samples.CartWorkflows");
        })
        .RunAsync(test);
}
