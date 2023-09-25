using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OrchardCore.Commerce.Tests.UI.Tests.ProductTests;

public class RetrievalProductTests : UITestBase
{
    public RetrievalProductTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Theory, Chrome]
    public Task HomePageShouldBeProductList(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                void TextShouldBe(string css, string expectedText) =>
                    context.Get(By.CssSelector(css)).Text.Trim().ShouldBe(expectedText);

                await context.SignInDirectlyAndGoToHomepageAsync();

                TextShouldBe(".site-heading h1", "My Shop");
                TextShouldBe(".content-price-variants-product header h2 a", "Test Price Variant Product");
                TextShouldBe(
                    ".content-price-variants-product > .field-name-product-" +
                    "part-product-image > .name",
                    "Product Image");
                TextShouldBe(".content-product header h2 a", "Test Product");
                TextShouldBe(
                    ".content-product > .field-name-product-" +
                    "part-product-image > .name",
                    "Product Image");

                context.GetAll(By.XPath("//img[@src='/media/ProductImages/sample-product-image.png']"))
                    .Count
                    .ShouldBe(5);
            },
            browser);
}
