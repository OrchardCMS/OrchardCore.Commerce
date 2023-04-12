using Lombiq.HelpfulLibraries.OrchardCore.Mvc;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OrchardCore.Commerce.Tests.UI.Shortcuts.Controllers;
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
                await context.SignInDirectlyAsync();
                await context.GoToContentItemByIdAsync(TestProduct);

                await context.ClickReliablyOnSubmitAsync();

                // Create order with successful payment.
                var orderCreateTime = DateTime.UtcNow.Ticks;
                await context.GoToAsync<OrderController>(controller =>
                    controller.CreateOrderWithSuccessfulPayment(orderCreateTime));

            },
            browser);
}
