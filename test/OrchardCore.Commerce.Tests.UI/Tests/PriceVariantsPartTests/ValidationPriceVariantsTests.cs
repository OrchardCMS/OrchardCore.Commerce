using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Xunit;

namespace OrchardCore.Commerce.Tests.UI.Tests.PriceVariantsPartTests;

public class ValidationPriceVariantsTests : UITestBase
{
    public ValidationPriceVariantsTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Theory, Chrome]
    public Task ProvidingExistingSkuShouldTripTheHandler(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
        {
            await context.SignInDirectlyAsync();
            await context.CreateNewContentItemAsync("PriceVariantsProduct");

            const string skuAlreadyExists = "TESTPRODUCTVARIANT";

            await context.ClickAndFillInWithRetriesAsync(By.Id("ProductPart_Sku"), skuAlreadyExists);

            await context.ClickReliablyOnSubmitAsync();

            context.ErrorMessageExists("SKU must be unique. A product with the given SKU already exists.");
        },
            browser);
}
