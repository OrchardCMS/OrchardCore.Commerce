using Lombiq.Tests.UI.Extensions;
using OpenQA.Selenium;
using Xunit;

namespace OrchardCore.Commerce.Tests.UI.Tests.PriceVariantsPartTests;

public class ValidationPriceVariantsTests : UITestBase
{
    public ValidationPriceVariantsTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Fact]
    public Task ProvidingExistingSkuShouldTripTheHandler() =>
        RunAsync(
            async context =>
        {
            await context.SignInDirectlyAsync();
            await context.CreateNewContentItemAsync("PriceVariantsProduct");

            const string skuAlreadyExists = "TESTPRODUCTVARIANT";

            await context.ClickAndFillInWithRetriesAsync(By.Id("ProductPart_Sku"), skuAlreadyExists);

            await context.ClickReliablyOnSubmitAsync();

            context.ErrorMessageExists("SKU must be unique. A product with the given SKU already exists.");
        });
}
