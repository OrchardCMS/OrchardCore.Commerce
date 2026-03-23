using Atata;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
using Xunit;

namespace OrchardCore.Commerce.Tests.UI.Tests.ProductTests;

public class SkuGeneratorTests : UITestBase
{
    public SkuGeneratorTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Theory, Chrome]
    public Task GuidSkuGeneratorShouldWork(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                string GetValue(string id) =>
                    context.Get(By.Id(id)).GetValue() ?? string.Empty;
                
                async Task CreateProductAsync(bool isSkuDisabled)
                {
                    await context.CreateNewContentItemAsync("Product", onlyIfNotAlreadyThere: false);
                    context
                        .Get(By.Id("ProductPart_Sku"))
                        .GetAttribute("readonly")
                        .ShouldBe(isSkuDisabled ? "true" : null);
                        
                    await context.ClickAndFillInWithRetriesAsync(By.Id("TitlePart_Title"), "SKU Test Product");
                    await context.ClickPublishAsync();
                }
                
                // Verify baseline product creation (SKU field must be filled manually).
                await context.SignInDirectlyAsync();
                await CreateProductAsync(isSkuDisabled: false);
                context.ErrorMessageExists("SKU must not be empty.");
                
                // Verify altered product creation (SKU field is disabled, value is auto-generated).
                await context.EnableFeatureDirectlyAsync(CommerceConstants.Features.SkuGeneratorGuid);
                await CreateProductAsync(isSkuDisabled: true);
                context.ShouldBeSuccess();

                // Verify published product.
                GetValue("TitlePart_Title").ShouldBe("SKU Test Product");
                GetValue("ProductPart_Sku").ToUpperInvariant().ShouldMatch("^[0-9A-F]{32}$");
            },
            browser);
}
