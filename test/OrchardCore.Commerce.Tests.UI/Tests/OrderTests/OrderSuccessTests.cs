using Lombiq.HelpfulLibraries.OrchardCore.Mvc;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using OrchardCore.Commerce.Tests.UI.Shortcuts.Controllers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using static OrchardCore.Commerce.Tests.UI.Constants.ContentItemIds;

namespace OrchardCore.Commerce.Tests.UI.Tests.OrderTests;

public class OrderSuccessTests : UITestBase
{
    public OrderSuccessTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Theory, Chrome]
    public Task ProductCanBeAddedToShoppingCart(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                await context.EnableFeatureDirectlyAsync("OrchardCore.Commerce.Tests.UI.Shortcuts");
                await context.SignInDirectlyAsync();

                await AddProductToCart(context, TestProduct);

                // Create order with successful payment.
                var orderCreateTime = DateTime.UtcNow.Ticks;
                await context.GoToAsync<OrderController>(controller =>
                    controller.CreateOrderWithSuccessfulPayment(orderCreateTime));
                context.Exists(By.XPath("//h4[contains(.,'Thank you for your purchase!')]"));
            },
            browser);

    [Theory, Chrome]
    public Task ShoppingCartUpdateButtonWorks(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {

                await context.EnableFeatureDirectlyAsync("OrchardCore.Commerce.Tests.UI.Shortcuts");
                await context.SignInDirectlyAsync();

                await AddProductToCart(context, TestProduct);
                await AddProductToCart(context, TestPriceVariantProduct);

                await context.ClickAndFillInWithRetriesAsync(QuantityFieldBy(1), "2");
                await context.ClickAndFillInWithRetriesAsync(QuantityFieldBy(2), "3");
                await context.ClickReliablyOnAsync(By.XPath("//button[contains(., 'Update')]"));

                context.Get(QuantityFieldBy(1)).GetAttribute("value").ShouldBeAsString(2);
                context.Get(QuantityFieldBy(2)).GetAttribute("value").ShouldBeAsString(3);
            },
            browser);

    private static async Task AddProductToCart(UITestContext context, string contentItemId)
    {
        await context.GoToContentItemByIdAsync(contentItemId);
        await context.ClickReliablyOnSubmitAsync();
    }

    private static By QuantityFieldBy(int number) =>
        By.XPath(FormattableString.Invariant(
            $"(//table[contains(@class, 'shopping-cart-table')]//input[contains(@name, '.Quantity')])[{number}]"));
}
