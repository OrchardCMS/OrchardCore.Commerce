using Atata;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
using System.Globalization;
using Xunit;
using Xunit.Abstractions;
using static OrchardCore.Commerce.Constants.ContentTypes;
using static OrchardCore.Commerce.Tests.UI.Constants.ContentItemIds;

namespace OrchardCore.Commerce.Tests.UI.Tests.OrderTests;

public class BehaviorOrderTests : UITestBase
{
    public BehaviorOrderTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Theory, Chrome]
    public Task OrderEditorShouldWorkProperly(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                await context.SignInDirectlyAndGoToDashboardAsync();
                await context.CreateNewContentItemAsync(Order);

                await AddNewProductAsync(context);

                // Total value should be 0 by default.
                AssertTotal(context, "$", "0");
                await context.ClickAndFillInWithRetriesAsync(ByQuantity(0), "5");
                await context.ClickAndFillInWithRetriesAsync(ByUnitPriceValue(0), "10");

                await AddNewProductAsync(context);

                await context.ClickAndFillInWithRetriesAsync(ByQuantity(1), "5");
                await context.ClickAndFillInWithRetriesAsync(ByUnitPriceValue(1), "10");

                // Total value should reflect values of Quantity and Unit Price Value fields.
                AssertTotal(context, "$", "100");

                // Displayed currency symbol should change based on topmost currency selector's value.
                await context.SetDropdownByTextAsync(ByUnitPriceCurrencyIsoCode(0), "EUR");
                AssertTotal(context, "€", "100");

                // Other currency selectors should not affect displayed currency symbol.
                await context.SetDropdownByTextAsync(ByUnitPriceCurrencyIsoCode(1), "USD");
                AssertTotal(context, "€", "100");

                // Product should be deletable from list before submitting it for the first time.
                await context.ClickReliablyOnAsync(By.XPath("//button[contains(@class, 'btn btn-danger')][1]"));
                context.Missing(ByQuantity(1));

                // Fill out required but otherwise irrelevant fields.
                await context.ClickAndFillInWithRetriesAsync(By.Name("OrderPart.Email.Text"), "test@email.com");
                await context.ClickAndFillInWithRetriesAsync(By.Name("OrderPart.Phone.Text"), "0123456789");
                await context.ClickAndFillInWithRetriesAsync(By.Name("OrderPart.BillingAddress.Address.Name"), "Test Name");
                await context.ClickAndFillInWithRetriesAsync(By.Name("OrderPart.BillingAddress.Address.Department"), "Test Department");
                await context.ClickAndFillInWithRetriesAsync(By.Name("OrderPart.BillingAddress.Address.Company"), "Test Company");
                await context.ClickAndFillInWithRetriesAsync(By.Name("OrderPart.BillingAddress.Address.StreetAddress1"), "Test First Street");
                await context.ClickAndFillInWithRetriesAsync(By.Name("OrderPart.BillingAddress.Address.StreetAddress2"), "Test Second Street");
                await context.ClickAndFillInWithRetriesAsync(By.Name("OrderPart.BillingAddress.Address.City"), "Test City");
                await context.ClickAndFillInWithRetriesAsync(By.Name("OrderPart.BillingAddress.Address.PostalCode"), "01234");
                await context.SetDropdownByTextAsync("OrderPart_BillingAddress_Address_Region", "United States");
                await context.ClickReliablyOnAsync(By.Name("OrderPart.BillingAndShippingAddressesMatch.Value"));
                await context.ClickPublishAsync();

                // Empty SKU field should result in validation errors being shown and no Product being added.
                context.ErrorMessageExists("Product's SKU cannot be left empty.");
                context.Missing(ByQuantity(0));

                await AddNewProductAsync(context);
                await context.ClickPublishAsync();

                // Submitting a completely empty Product should result in no Product being added.
                context.Missing(ByQuantity(0));

                await AddNewProductAsync(context);

                await context.ClickAndFillInWithRetriesAsync(ByQuantity(0), "5");
                await context.ClickAndFillInWithRetriesAsync(ByProductSku(0), "nonexistentproduct"); // #spell-check-ignore-line
                await context.ClickAndFillInWithRetriesAsync(ByUnitPriceValue(0), "10");
                await context.ClickPublishAsync();

                // Non-existent SKU should result in validation errors being shown and no Product being added.
                context.ErrorMessageExists("SKU \"NONEXISTENTPRODUCT\" does not belong to an existing Product."); // #spell-check-ignore-line

                context.Missing(ByQuantity(0));
                await AddNewProductAsync(context);

                await context.ClickAndFillInWithRetriesAsync(ByQuantity(0), "5");
                await context.ClickAndFillInWithRetriesAsync(ByProductSku(0), "testproductvariant");
                await context.ClickAndFillInWithRetriesAsync(ByUnitPriceValue(0), "10");
                await context.SetDropdownByTextAsync(ByUnitPriceCurrencyIsoCode(0), "EUR");
                await context.ClickPublishAsync();

                // Selected currency should be saved.
                AssertTotal(context, "€", "50");
                context
                    .Get(ByUnitPriceCurrencyIsoCode(0))
                    .GetValue()
                    .ShouldBe("EUR");

                // For a Product with existing attributes, attributes should be populated and should be selectable.
                await context.SetDropdownByTextAsync(By.Name("OrderPart.LineItems[0].SelectedAttributes[Size]"), "Medium");
                await context.ClickPublishAsync();

                context
                    .Get(By.Name("OrderPart.LineItems[0].SelectedAttributes[Size]"))
                    .GetValue()
                    .ShouldBe("Medium");

                await AddNewProductAsync(context);

                await context.ClickAndFillInWithRetriesAsync(ByQuantity(1), "5");
                await context.ClickAndFillInWithRetriesAsync(ByProductSku(1), "testproduct");
                await context.ClickAndFillInWithRetriesAsync(ByUnitPriceValue(1), "10");
                await context.ClickPublishAsync();

                // No attributes should be loaded for Product without attributes.
                context.Missing(By.Name("OrderPart.LineItems[1].SelectedAttributes[Size]"));

                await context.SetDropdownByTextAsync(ByUnitPriceCurrencyIsoCode(1), "CAD");
                await context.ClickPublishAsync();

                // Selecting non-matching currencies should result in validation errors.
                context.ErrorMessageExists("Selected currencies must match.");

                // Links in Product Name cells should lead to content item editors.
                context
                    .Get(By.XPath("//a[contains(., 'Test Price Variant Product')]"))
                    .GetAttribute("href")
                    .ShouldContain($"/Admin/Contents/ContentItems/{TestPriceVariantProduct}/Edit");

                context
                    .Get(By.XPath("//a[contains(., 'Test Product')]"))
                    .GetAttribute("href")
                    .ShouldContain($"/Admin/Contents/ContentItems/{TestProduct}/Edit");

                // Product should be deletable from list after it has been submitted.
                await context.ClickReliablyOnAsync(By.XPath("//button[contains(@class, 'btn btn-danger')][1]"));
                await context.ClickPublishAsync();

                context.Missing(ByQuantity(1));
            },
            browser);

    private static Task AddNewProductAsync(UITestContext context) => context.ClickReliablyOnAsync(By.Id("addButton"));

    private static void AssertTotal(UITestContext context, string currencySymbol, string totalValue) =>
        context.Exists(By.XPath($"//strong[contains(., '{currencySymbol} {totalValue}')]"));

    private static By ByQuantity(int index) =>
        By.Name(string.Create(CultureInfo.InvariantCulture, $"OrderPart.LineItems[{index}].Quantity"));

    private static By ByProductSku(int index) =>
        By.Name(string.Create(CultureInfo.InvariantCulture, $"OrderPart.LineItems[{index}].ProductSku"));

    private static By ByUnitPriceValue(int index) =>
        By.Name(string.Create(CultureInfo.InvariantCulture, $"OrderPart.LineItems[{index}].UnitPriceValue"));

    private static By ByUnitPriceCurrencyIsoCode(int index) =>
        By.Name(string.Create(CultureInfo.InvariantCulture, $"OrderPart.LineItems[{index}].UnitPriceCurrencyIsoCode"));
}
