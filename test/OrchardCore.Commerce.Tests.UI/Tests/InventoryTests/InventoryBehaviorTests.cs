using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using System.Globalization;
using Xunit;
using Xunit.Abstractions;
using static OrchardCore.Commerce.Tests.UI.Constants.ContentItemIds;

namespace OrchardCore.Commerce.Tests.UI.Tests.PromotionTests;

public class InventoryBehaviorTests : UITestBase
{
    public InventoryBehaviorTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    public const string CheckoutUnavailableMessage = "Checkout unavailable — invalid item in cart.";

    [Theory, Chrome]
    public Task InventoryChecksOnCartUpdateShouldWorkProperly(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                await context.SignInDirectlyAndGoToRelativeUrlAsync("/testproduct");
                await context.ClickReliablyOnAsync(By.XPath("//button[contains(., 'Add to cart')]"));

                // When quantity is set to a value below the possible max quantity, error message should not appear.
                await UpdateCartAndAssertErrorsAsync(
                    context,
                    2,
                    "The checkout quantity for Test Product is more than the maximum allowed (2).",
                    shouldExist: false);

                await context.GoToContentItemEditorByIdAsync(TestProduct);
                await context.ClickReliablyOnAsync(By.Id("InventoryPart_AllowsBackOrder_Value"));
                await context.ClickAndFillInWithRetriesAsync(By.Id("InventoryPart_MinimumOrderQuantity_Value"), "3");
                await context.ClickAndFillInWithRetriesAsync(By.Id("InventoryPart_MaximumOrderQuantity_Value"), "10");
                await context.ClickPublishAsync();
                await context.GoToCartAsync();

                // When quantity is set to a value above the minimum possible quantity, error message should not appear.
                await UpdateCartAndAssertErrorsAsync(
                    context,
                    4,
                    "The checkout quantity for Test Product is less than the minimum allowed (3).",
                    shouldExist: false);

                // When quantity is set to a value above the current inventory value, error message should appear.
                await UpdateCartAndAssertErrorsAsync(
                    context,
                    10,
                    "There are not enough Test Product left in stock.",
                    shouldExist: true);

                await context.GoToContentItemEditorByIdAsync(TestProduct);
                await context.ClickReliablyOnAsync(By.Id("InventoryPart_IgnoreInventory_Value"));
                await context.ClickPublishAsync();
                await context.GoToCartAsync();

                // When inventory is ignored, inventory checks should not apply.
                await UpdateCartAndAssertErrorsAsync(
                    context,
                    2,
                    "The checkout quantity for Test Product is less than the minimum allowed (3).",
                    shouldExist: false);

                await UpdateCartAndAssertErrorsAsync(
                    context,
                    15,
                    "The checkout quantity for Test Product is more than the maximum allowed (10).",
                    shouldExist: false);
                AssertError(context, "There are not enough Test Product left in stock.", shouldExist: false);
            },
            browser);

    [Theory, Chrome]
    public Task UnavailableProductInCartShouldPreventCheckout(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                await context.SignInDirectlyAndGoToRelativeUrlAsync("/testproduct");
                await context.ClickReliablyOnAsync(By.XPath("//button[contains(., 'Add to cart')]"));

                // Set Inventory of product in cart to 0 and verify checkout is unavailable.
                await context.GoToContentItemEditorByIdAsync(TestProduct);
                await context.ClickReliablyOnAsync(By.XPath("//label[contains(., 'Allows Back Order')]"));
                await context.ClickAndFillInWithRetriesAsync(By.Name("InventoryPart.Inventory[TESTPRODUCT]"), "0");
                await context.ClickPublishAsync();

                await VerifyCheckoutAvailabilityAsync(context, cartMessageShouldExist: true, checkoutMessageShouldExist: true);

                // Add an available product to cart and verify checkout is still not possible.
                await context.GoToRelativeUrlAsync("testfreeproduct");
                await context.ClickReliablyOnAsync(By.XPath("//button[contains(., 'Add to cart')]"));
                await VerifyCheckoutAvailabilityAsync(
                    context, cartMessageShouldExist: true, checkoutMessageShouldExist: true, navigate: false);

                // Set Ignore Inventory to true and verify checkout is available.
                await context.GoToContentItemEditorByIdAsync(TestProduct);
                await context.ClickReliablyOnAsync(By.XPath("//label[contains(., 'Ignore Inventory')]"));
                await context.ClickPublishAsync();

                await VerifyCheckoutAvailabilityAsync(context, cartMessageShouldExist: false, checkoutMessageShouldExist: false);

                // Set Allows Back Order to true and verify checkout is available.
                await context.GoToContentItemEditorByIdAsync(TestProduct);
                await context.ClickReliablyOnAsync(By.XPath("//label[contains(., 'Ignore Inventory')]"));
                await context.ClickReliablyOnAsync(By.XPath("//label[contains(., 'Allows Back Order')]"));
                await context.ClickPublishAsync();

                await VerifyCheckoutAvailabilityAsync(context, cartMessageShouldExist: false, checkoutMessageShouldExist: false);
            },
            browser);

    private static async Task VerifyCheckoutAvailabilityAsync(
        UITestContext context, bool cartMessageShouldExist, bool checkoutMessageShouldExist, bool navigate = true)
    {
        if (navigate)
        {
            await context.GoToCartAsync();
        }

        context.CheckExistence(By.ClassName("checkout-unavailable"), cartMessageShouldExist);
        await context.GoToCheckoutAsync();
        context.CheckExistence(
            By.XPath($"//div[contains(@class, 'message-error') and contains(., '{CheckoutUnavailableMessage}')]"),
            checkoutMessageShouldExist);
    }

    private static async Task UpdateCartAndAssertErrorsAsync(
        UITestContext context,
        int quantity,
        string errorMessage,
        bool shouldExist = true)
    {
        await context.ClickAndFillInWithRetriesAsync(QuantityFieldBy(1), quantity.ToString(CultureInfo.InvariantCulture));
        await context.ClickReliablyOnAsync(By.XPath("//button[contains(., 'Update')]"));
        AssertError(context, errorMessage, shouldExist);
    }

    private static void AssertError(UITestContext context, string errorMessage, bool shouldExist = true)
    {
        var errorDivXPath = $"//div[contains(., '{errorMessage}')]";

        if (shouldExist)
        {
            context.Exists(By.XPath(errorDivXPath));
        }
        else
        {
            context.Missing(By.XPath(errorDivXPath));
        }
    }

    private static By QuantityFieldBy(int number) =>
        By.XPath(string.Create(
            CultureInfo.InvariantCulture,
            $"(//table[contains(@class, 'shopping-cart-table')]//input[contains(@name, '.Quantity')])[{number}]"));
}
