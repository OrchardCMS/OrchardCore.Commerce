using Atata;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Xunit;
using Xunit.Abstractions;
using static OrchardCore.Commerce.Tests.UI.Constants.ContentItemIds;

namespace OrchardCore.Commerce.Tests.UI.Tests.PromotionTests;

public class InventoryBehaviourTests : UITestBase
{
    public const string PriceFieldCssSelector = ".price-part-price-field-value";

    public InventoryBehaviourTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Theory, Chrome]
    public Task InventoryChecksOnCartUpdateShouldWorkProperly(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                await context.SignInDirectlyAndGoToRelativeUrlAsync("/testproduct");
                await context.ClickReliablyOnAsync(By.XPath("//button[contains(., 'Add to cart')]"));

                // When quantity is set to a value below the possible max quantity, error message should not appear.
                await context.ClickAndFillInWithRetriesAsync(
                    By.XPath("//input[@class='form-control shopping-cart-table-quantity']"), "2");
                await context.ClickReliablyOnAsync(By.XPath("//button[contains(., 'Update')]"));
                context.Missing(By.XPath(
                    "//div[contains(., 'The checkout quantity for Test Product is more than the maximum allowed (2).')]"));

                // When quantity is set to a value above the maximum possible quantity, error message should appear.
                await context.ClickAndFillInWithRetriesAsync(
                    By.XPath("//input[@class='form-control shopping-cart-table-quantity']"), "5");
                await context.ClickReliablyOnAsync(By.XPath("//button[contains(., 'Update')]"));
                context.Exists(By.XPath(
                    "//div[contains(., 'The checkout quantity for Test Product is more than the maximum allowed (2).')]"));

                await context.GoToAdminRelativeUrlAsync($"/Contents/ContentItems/{TestProduct}/Edit");
                await context.ClickReliablyOnAsync(By.XPath("//input[@id='InventoryPart_AllowsBackOrder_Value']"));
                await context.ClickAndFillInWithRetriesAsync(
                    By.XPath("//input[@id='InventoryPart_MinimumOrderQuantity_Value']"), "3");
                await context.ClickAndFillInWithRetriesAsync(
                    By.XPath("//input[@id='InventoryPart_MaximumOrderQuantity_Value']"), "10");
                await context.ClickPublishAsync();

                await context.GoToRelativeUrlAsync("/cart");

                // When quantity is set to a value below the minimum possible quantity, error message should appear.
                await context.ClickAndFillInWithRetriesAsync(
                    By.XPath("//input[@class='form-control shopping-cart-table-quantity']"), "2");
                await context.ClickReliablyOnAsync(By.XPath("//button[contains(., 'Update')]"));
                context.Exists(By.XPath(
                    "//div[contains(., 'The checkout quantity for Test Product is less than the minimum allowed (3).')]"));

                // When quantity is set to a value above the minimum possible quantity, error message should not appear.
                await context.ClickAndFillInWithRetriesAsync(
                    By.XPath("//input[@class='form-control shopping-cart-table-quantity']"), "4");
                await context.ClickReliablyOnAsync(By.XPath("//button[contains(., 'Update')]"));
                context.Missing(By.XPath(
                    "//div[contains(., 'The checkout quantity for Test Product is less than the minimum allowed (3).')]"));

                // When quantity is set to a value above the current inventory value, error message should appear.
                await context.ClickAndFillInWithRetriesAsync(
                    By.XPath("//input[@class='form-control shopping-cart-table-quantity']"), "10");
                await context.ClickReliablyOnAsync(By.XPath("//button[contains(., 'Update')]"));
                context.Exists(By.XPath(
                    "//div[contains(., 'There are not enough Test Product left in stock.')]"));

                await context.GoToAdminRelativeUrlAsync($"/Contents/ContentItems/{TestProduct}/Edit");
                await context.ClickReliablyOnAsync(By.XPath("//input[@id='InventoryPart_IgnoreInventory_Value']"));
                await context.ClickPublishAsync();

                await context.GoToRelativeUrlAsync("/cart");

                // When inventory is ignored, inventory checks should not apply.
                await context.ClickAndFillInWithRetriesAsync(
                    By.XPath("//input[@class='form-control shopping-cart-table-quantity']"), "2");
                await context.ClickReliablyOnAsync(By.XPath("//button[contains(., 'Update')]"));
                context.Missing(By.XPath(
                    "//div[contains(., 'The checkout quantity for Test Product is less than the minimum allowed (3).')]"));

                await context.ClickAndFillInWithRetriesAsync(
                    By.XPath("//input[@class='form-control shopping-cart-table-quantity']"), "15");
                await context.ClickReliablyOnAsync(By.XPath("//button[contains(., 'Update')]"));
                context.Missing(By.XPath(
                    "//div[contains(., 'The checkout quantity for Test Product is more than the maximum allowed (10).')]"));
                context.Missing(By.XPath(
                    "//div[contains(., 'There are not enough Test Product left in stock.')]"));
            },
            browser);
}
