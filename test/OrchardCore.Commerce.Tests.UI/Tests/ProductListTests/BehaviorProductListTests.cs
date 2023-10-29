using Atata.HtmlValidation;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OrchardCore.Commerce.Tests.UI.Tests.ProductListTests;

public class BehaviorProductListTests : UITestBase
{
    public BehaviorProductListTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Theory, Chrome]
    public Task SortingByTitleShouldWork(Browser browser) =>
        ExecuteTestAfterSetupAsync(
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
            },
            browser,
            IgnorePagerHtmlPaginationErrors);

    [Theory, Chrome]
    public Task FilteringByTitleShouldWork(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                await context.FillInWithRetriesAsync(By.Id("products-filter-title"), "tiered");
                await context.ClickReliablyOnSubmitAsync();

                var titles = GetProductTitles(context);

                titles.Count.ShouldBe(1);
                titles[0].ShouldBe("Test Tiered Price Product");
            },
            browser,
            IgnorePagerHtmlPaginationErrors);

    [Theory, Chrome]
    public Task SortingAndPaginationShouldWorkTogether(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                await context.SetDropdownByTextAsync("products-order-by", "Title Z-A");
                await context.ClickReliablyOnSubmitAsync();

                await context.ClickReliablyOnAsync(By.CssSelector(".pager li.last a"));

                var titles = GetProductTitles(context);

                titles.Count.ShouldBe(1);
                titles[0].ShouldBe("Test Discounted Product");
            },
            browser,
            IgnorePagerHtmlPaginationErrors);

    private static readonly Func<OrchardCoreUITestExecutorConfiguration, Task>
        IgnorePagerHtmlPaginationErrors =
            configuration =>
            {
                configuration.HtmlValidationConfiguration.AssertHtmlValidationResultAsync =
                    AssertHtmValidationResultAsync;

                return Task.CompletedTask;
            };

    private static async Task AssertHtmValidationResultAsync(HtmlValidationResult validationResult)
    {
        var errors = (await validationResult.GetErrorsAsync())
            .Where(error => !error.ContainsOrdinalIgnoreCase("\"rel\" attribute cannot be used in this context"));
        errors.ShouldBeEmpty();
    }

    private static IList<string> GetProductTitles(UITestContext context) =>
        context
            .GetAll(By.CssSelector(".content header h2 a"))
            .Select(e => e.Text)
            .ToList();
}
