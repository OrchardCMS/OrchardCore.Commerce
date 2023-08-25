using Atata;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

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
                await context.SignInDirectlyAsync();
                await context.GoToDashboardAsync();
                await context.CreateNewContentItemAsync("Order");

                await context.ClickReliablyOnAsync(By.Id("addButton"));

                // Total value should be 0 by default.
                context.Exists(By.XPath("//strong[contains(., '$ 0')]"));
                await context.ClickAndFillInWithRetriesAsync(By.Name("OrderPart.LineItems[0].Quantity"), "5");
                await context.ClickAndFillInWithRetriesAsync(By.Name("OrderPart.LineItems[0].UnitPriceValue"), "10");

                await context.ClickReliablyOnAsync(By.Id("addButton"));

                await context.ClickAndFillInWithRetriesAsync(By.Name("OrderPart.LineItems[1].Quantity"), "5");
                await context.ClickAndFillInWithRetriesAsync(By.Name("OrderPart.LineItems[1].UnitPriceValue"), "10");

                // Total value should reflect values of Quantity and Unit Price Value fields.
                context.Exists(By.XPath("//strong[contains(., '$ 100')]"));

                // Displayed currency symbol should change based on topmost currency selector's value.
                await context.SetDropdownByTextAsync(By.Name("OrderPart.LineItems[0].UnitPriceCurrencyIsoCode"), "EUR");
                context.Exists(By.XPath("//strong[contains(., '€ 100')]"));

                // Other currency selectors should not affect displayed currency symbol.
                await context.SetDropdownByTextAsync(By.Name("OrderPart.LineItems[1].UnitPriceCurrencyIsoCode"), "USD");
                context.Exists(By.XPath("//strong[contains(., '€ 100')]"));

                // Product should be deletable from list before submitting it for the first time.
                await context.ClickReliablyOnAsync(By.XPath("//button[contains(@class, 'btn btn-danger')][1]"));
                context.Missing(By.Id("OrderPart.LineItems[1].Quantity"));

                // Fill out required but otherwise irrelevant fields.
                await context.ClickAndFillInWithRetriesAsync(By.Id("OrderPart_Email_Text"), "test@email.com");
                await context.ClickAndFillInWithRetriesAsync(By.Id("OrderPart_Phone_Text"), "0123456789");
                await context.ClickAndFillInWithRetriesAsync(By.Id("OrderPart_BillingAddress_Address_Name"), "Test Name");
                await context.ClickAndFillInWithRetriesAsync(By.Id("OrderPart_BillingAddress_Address_Department"), "Test Department");
                await context.ClickAndFillInWithRetriesAsync(By.Id("OrderPart_BillingAddress_Address_Company"), "Test Company");
                await context.ClickAndFillInWithRetriesAsync(By.Id("OrderPart_BillingAddress_Address_StreetAddress1"), "Test First Street");
                await context.ClickAndFillInWithRetriesAsync(By.Id("OrderPart_BillingAddress_Address_StreetAddress2"), "Test Second Street");
                await context.ClickAndFillInWithRetriesAsync(By.Id("OrderPart_BillingAddress_Address_City"), "Test City");
                await context.ClickAndFillInWithRetriesAsync(By.Id("OrderPart_BillingAddress_Address_PostalCode"), "01234");
                await context.SetDropdownByTextAsync("OrderPart_BillingAddress_Address_Region", "United States");
                await context.ClickReliablyOnAsync(By.Id("OrderPart_BillingAndShippingAddressesMatch_Value"));
                await context.ClickPublishAsync();

                // Empty SKU field should result in validation errors being shown and no Product being added.
                context.Exists(By.XPath("//div[contains(@class, 'validation-summary-errors')]" +
                    "[contains(., 'SKU \"\" is empty or does not belong to an existing Product.')]"));
                context.Missing(By.Id("OrderPart.LineItems[0].Quantity"));

                await context.ClickReliablyOnAsync(By.Id("addButton"));
                await context.ClickPublishAsync();

                // Submitting a completely empty Product should result in no Product being added.
                context.Missing(By.Id("OrderPart.LineItems[0].Quantity"));

                await context.ClickReliablyOnAsync(By.Id("addButton"));

                await context.ClickAndFillInWithRetriesAsync(By.Name("OrderPart.LineItems[0].Quantity"), "5");
                await context.ClickAndFillInWithRetriesAsync(
                    By.Name("OrderPart.LineItems[0].ProductSku"), "nonexistentproduct"); // #spell-check-ignore-line
                await context.ClickAndFillInWithRetriesAsync(By.Name("OrderPart.LineItems[0].UnitPriceValue"), "10");
                await context.ClickPublishAsync();

                // Non-existent SKU should result in validation errors being shown and no Product being added.
                context.Exists(By.XPath("//div[contains(@class, 'validation-summary-errors')]" +
                    "[contains(., 'SKU \"NONEXISTENTPRODUCT\" is empty or does not belong to an existing Product.')]")); // #spell-check-ignore-line
                context.Missing(By.Id("OrderPart.LineItems[0].Quantity"));

                await context.ClickReliablyOnAsync(By.Id("addButton"));

                await context.ClickAndFillInWithRetriesAsync(By.Name("OrderPart.LineItems[0].Quantity"), "5");
                await context.ClickAndFillInWithRetriesAsync(By.Name("OrderPart.LineItems[0].ProductSku"), "testproductvariant");
                await context.ClickAndFillInWithRetriesAsync(By.Name("OrderPart.LineItems[0].UnitPriceValue"), "10");
                await context.ClickPublishAsync();

                // For a Product with existing attributes, attributes should be populated and should be selectable.
                await context.SetDropdownByTextAsync(By.Name("OrderPart.LineItems[0].SelectedAttributes[Size]"), "Medium");
                await context.ClickPublishAsync();

                context
                    .Get(By.Name("OrderPart.LineItems[0].SelectedAttributes[Size]"))
                    .GetValue()
                    .ShouldBe("Medium");

                await context.ClickReliablyOnAsync(By.Id("addButton"));

                await context.ClickAndFillInWithRetriesAsync(By.Name("OrderPart.LineItems[1].Quantity"), "5");
                await context.ClickAndFillInWithRetriesAsync(By.Name("OrderPart.LineItems[1].ProductSku"), "testproduct");
                await context.ClickAndFillInWithRetriesAsync(By.Name("OrderPart.LineItems[1].UnitPriceValue"), "10");
                await context.ClickPublishAsync();

                // No attributes should be loaded for Product without attributes.
                context.Missing(By.Name("OrderPart.LineItems[1].SelectedAttributes[Size]"));

                await context.SetDropdownByTextAsync(By.Name("OrderPart.LineItems[1].UnitPriceCurrencyIsoCode"), "CAD");
                await context.ClickPublishAsync();

                // Selecting non-matching currencies should result in validation errors.
                context.Exists(By.XPath("//div[contains(@class, 'validation-summary-errors')]" +
                    "[contains(., 'Selected currencies need to match.')]"));

                // Links in Product Name cells should lead to content item editors.
                context
                    .Get(By.XPath("//a[contains(., 'Test Price Variant Product')]"))
                    .GetAttribute("href")
                    .ShouldContain("/Admin/Contents/ContentItems/testpricevariantproduct000/Edit");

                context
                    .Get(By.XPath("//a[contains(., 'Test Product')]"))
                    .GetAttribute("href")
                    .ShouldContain("/Admin/Contents/ContentItems/testproduct000/Edit");

                // Product should be deletable from list after it has been submitted.
                await context.ClickReliablyOnAsync(By.XPath("//button[contains(@class, 'btn btn-danger')][1]"));
                await context.ClickPublishAsync();

                context.Missing(By.Id("OrderPart.LineItems[1].Quantity"));
            },
            browser);
}
