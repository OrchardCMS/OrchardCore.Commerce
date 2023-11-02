using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using static OrchardCore.Commerce.Tests.UI.Constants.ContentItemIds;

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
                TextShouldBe(".content-product header h2 a", "Test Discounted Product");
                TextShouldBe(
                    ".content-product > .field-name-product-" +
                    "part-product-image > .name",
                    "Product Image");

                context.GetAll(By.XPath("//img[@src='/media/ProductImages/sample-product-image.png']"))
                    .Count
                    .ShouldBe(5);
            },
            browser);

    [Theory, Chrome]
    public Task ProductAttributesShouldBeDisplayedCorrectly(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                await context.SignInDirectlyAsync();
                await context.GoToAdminRelativeUrlAsync("/ContentParts/PriceVariantsProduct/Fields/Size/Edit");
                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("PriceVariantsProduct_Size_TextProductAttributeFieldSettingsDriver_Hint"),
                    "Test Text Hint");
                await context.ClickReliablyOnSubmitAsync();

                // Verify text attribute properties show up correctly.
                await context.GoToContentItemByIdAsync(TestPriceVariantProduct);
                await context.ClickReliablyOnAsync(By.XPath("//label[contains(., 'Medium')]"));
                context.Exists(By.XPath("//div[contains(., 'Test Text Hint')]"));
                await context.ClickReliablyOnSubmitAsync();

                context.Exists(By.XPath("//li[contains(., 'Size: Medium')]"));
                await context.ClickReliablyOnAsync(By.XPath("//button[contains(., 'Remove')]"));

                // Add new boolean attribute and verify its properties show up correctly.
                await context.GoToAddFieldToContentTypeAsync("PriceVariantsProduct");
                await context.ClickAndFillInWithRetriesAsync(By.Id("DisplayName"), "TestBooleanAttribute");
                await context.ClickReliablyOnAsync(By.XPath("//label[contains(., 'Boolean Product Attribute Field')]"));
                await context.ClickReliablyOnSubmitAsync();

                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("PriceVariantsProduct_TestBooleanAttribute_BooleanProductAttributeFieldSettingsDriver_Hint"),
                    "Test Boolean Hint");
                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("PriceVariantsProduct_TestBooleanAttribute_BooleanProductAttributeFieldSettingsDriver_Label"),
                    "Test Boolean Label");
                await context.ClickReliablyOnAsync(By.XPath("//label[contains(., 'On/Off')]"));
                await context.ClickReliablyOnSubmitAsync();

                await context.GoToContentItemByIdAsync(TestPriceVariantProduct);
                context.Exists(By.XPath("//label[contains(., 'Test Boolean Label')]"));
                context.Exists(By.XPath("//div[contains(., 'Test Boolean Hint')]"));
                await context.ClickReliablyOnSubmitAsync();

                context.Exists(By.XPath("//li[contains(., 'Size: Small')]"));
                context.Exists(By.XPath("//li[contains(., 'Test Boolean Label: Yes')]"));
                await context.ClickReliablyOnAsync(By.XPath("//button[contains(., 'Remove')]"));

                // Add new numeric attribute and verify its properties show up correctly.
                await context.GoToAddFieldToContentTypeAsync("PriceVariantsProduct");
                await context.ClickAndFillInWithRetriesAsync(By.Id("DisplayName"), "TestNumericAttribute");
                await context.ClickReliablyOnAsync(By.XPath("//label[contains(., 'Numeric Product Attribute Field')]"));
                await context.ClickReliablyOnSubmitAsync();

                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("PriceVariantsProduct_TestNumericAttribute_NumericProductAttributeFieldSettingsDriver_Hint"),
                    "Test Numeric Hint");
                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("PriceVariantsProduct_TestNumericAttribute_NumericProductAttributeFieldSettingsDriver_Placeholder"),
                    "Test Numeric Placeholder");
                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("PriceVariantsProduct_TestNumericAttribute_NumericProductAttributeFieldSettingsDriver_Minimum"),
                    "1");
                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("PriceVariantsProduct_TestNumericAttribute_NumericProductAttributeFieldSettingsDriver_Maximum"),
                    "2");
                await context.ClickReliablyOnSubmitAsync();

                await context.GoToContentItemByIdAsync(TestPriceVariantProduct);
                context.Exists(By.XPath("//div[contains(., 'Test Numeric Hint')]"));
                context
                    .Get(By.Id("product-TESTPRODUCTVARIANT-attribute-2"))
                    .GetAttribute("placeholder")
                    .ShouldBe("Test Numeric Placeholder");
                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("product-TESTPRODUCTVARIANT-attribute-2"),
                    "2");
                await context.ClickReliablyOnSubmitAsync();
                context.Exists(By.XPath("//li[contains(., 'TestNumericAttribute: 2')]"));
            },
            browser);
}
