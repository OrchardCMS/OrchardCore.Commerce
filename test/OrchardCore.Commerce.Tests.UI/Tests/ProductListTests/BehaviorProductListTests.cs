using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
using Xunit;

namespace OrchardCore.Commerce.Tests.UI.Tests.ProductListTests;

public class BehaviorProductListTests : UITestBase
{
    public BehaviorProductListTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Fact]
    public Task SortingByTitleShouldWork() =>
        RunAsync(
            async context =>
            {
                await context.SetDropdownByTextAsync("products-order-by", "Title A-Z");
                await context.ClickReliablyOnSubmitAsync();

                var titles = GetProductTitles(context);

                titles[0].ShouldBe("Test Discounted Product");
                titles[1].ShouldBe("Test Free Product");

                await context.SetDropdownByTextAsync("products-order-by", "Title Z-A");
                await context.ClickReliablyOnSubmitAsync();

                titles = GetProductTitles(context);

                titles[0].ShouldBe("Test Tiered Price Product");
                titles[1].ShouldBe("Test Product");
            });

    [Fact]
    public Task FilteringByTitleShouldWork() =>
        RunAsync(
            async context =>
            {
                await context.FillInWithRetriesAsync(By.Id("products-filter-title"), "tiered");
                await context.ClickReliablyOnSubmitAsync();

                var titles = GetProductTitles(context);

                titles.Count.ShouldBe(1);
                titles[0].ShouldBe("Test Tiered Price Product");
            });

    [Fact]
    public Task SortingAndPaginationShouldWorkTogether() =>
        RunAsync(
            async context =>
            {
                await context.SetDropdownByTextAsync("products-order-by", "Title Z-A");
                await context.ClickReliablyOnSubmitAsync();

                await context.ClickReliablyOnAsync(By.CssSelector(".pager li.last a"));

                var titles = GetProductTitles(context);

                titles.Count.ShouldBe(1);
                titles[0].ShouldBe("Test Discounted Product");
            });

    private static List<string> GetProductTitles(UITestContext context) =>
        context
            .GetAll(By.CssSelector(".content header h2 a"))
            .Select(element => element.Text)
            .ToList();
}
