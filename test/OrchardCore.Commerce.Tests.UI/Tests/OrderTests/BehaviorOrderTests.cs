using Atata;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
using System.Globalization;
using Xunit;
using Xunit.Abstractions;
using static OrchardCore.Commerce.Tests.UI.Constants.ContentItemIds;

namespace OrchardCore.Commerce.Tests.UI.Tests.OrderTests;

public class BehaviorOrderTests : UITestBase
{
    public BehaviorOrderTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    public const string CompletePaymentButtonXPath =
        "//a[@class='pay-button btn btn-success justify-content-center' and contains(., 'Complete Payment')]";

    [Theory, Chrome]
    public Task OrderEditorShouldWorkProperly(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                await context.SignInDirectlyAndGoToDashboardAsync();
                await context.GoToContentItemEditorByIdAsync(TestOrder);

                await ClickDeleteItemAsync(context, 1);
                await ClickAddItemAsync(context);

                // Total value should be 0 by default.
                AssertTotal(context, "$", "0");
                await context.ClickAndFillInWithRetriesAsync(ByQuantity(0), "5");
                await context.ClickAndFillInWithRetriesAsync(ByUnitPriceValue(0), "10");

                await ClickAddItemAsync(context);

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
                await ClickDeleteItemAsync(context, 1);
                context.Missing(ByQuantity(1));

                // Fill out required but otherwise irrelevant fields.
                await context.ClickPublishAsync();

                // Empty SKU field should result in validation errors being shown and no Product being added.
                context.ErrorMessageExists("Product's SKU cannot be left empty.");
                context.Missing(ByQuantity(0));

                await ClickAddItemAsync(context);
                await context.ClickPublishAsync();

                // Submitting a completely empty Product should result in no Product being added.
                context.Missing(ByQuantity(0));

                await ClickAddItemAsync(context);

                await context.ClickAndFillInWithRetriesAsync(ByQuantity(0), "5");
                await context.ClickAndFillInWithRetriesAsync(ByProductSku(0), "nonexistentproduct"); // #spell-check-ignore-line
                await context.ClickAndFillInWithRetriesAsync(ByUnitPriceValue(0), "10");
                await context.ClickPublishAsync();

                // Non-existent SKU should result in validation errors being shown and no Product being added.
                context.ErrorMessageExists("SKU \"NONEXISTENTPRODUCT\" does not belong to an existing Product."); // #spell-check-ignore-line

                context.Missing(ByQuantity(0));
                await ClickAddItemAsync(context);

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
                await context.SetDropdownByTextAsync(By.Name("OrderPart.LineItems[0].SelectedTextAttributes[Size]"), "Medium");
                await context.ClickPublishAsync();

                context
                    .Get(By.Name("OrderPart.LineItems[0].SelectedTextAttributes[Size]"))
                    .GetValue()
                    .ShouldBe("Medium");

                await ClickAddItemAsync(context);

                await context.ClickAndFillInWithRetriesAsync(ByQuantity(1), "5");
                await context.ClickAndFillInWithRetriesAsync(ByProductSku(1), "testproduct");
                await context.ClickAndFillInWithRetriesAsync(ByUnitPriceValue(1), "10");
                await context.ClickPublishAsync();

                // No attributes should be loaded for Product without attributes.
                context.Missing(By.Name("OrderPart.LineItems[1].SelectedTextAttributes[Size]"));

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
                await ClickDeleteItemAsync(context, 1);
                await context.ClickPublishAsync();

                context.Missing(ByQuantity(1));
            },
            browser);

    [Theory, Chrome]
    public Task OrderPaymentRequestButtonShouldWorkProperly(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                await context.SignInDirectlyAsync();

                // Complete Payment button should be present if there are line items that require payment.
                await context.GoToRelativeUrlAsync($"/Contents/ContentItems/{TestOrder}");
                var completePaymentButton = context.Get(By.XPath(CompletePaymentButtonXPath));
                completePaymentButton.GetAttribute("href").ShouldContain($"checkout/paymentrequest/{TestOrder}");

                // Complete Payment button should not show up if Order is not Pending.
                await context.GoToContentItemEditorByIdAsync(TestOrder);
                await context.ClickReliablyOnAsync(By.Id("OrderPart_Status_Text_1"));
                await context.ClickPublishAsync();

                await context.GoToRelativeUrlAsync($"/Contents/ContentItems/{TestOrder}");
                context.Missing(By.XPath(CompletePaymentButtonXPath));

                // Complete Payment button should not show up if Order has no line items.
                await context.GoToContentItemEditorByIdAsync(TestOrder);
                await ClickDeleteItemAsync(context, 1);
                await context.ClickReliablyOnAsync(By.Id("OrderPart_Status_Text_0"));
                await context.ClickPublishAsync();

                await context.GoToRelativeUrlAsync($"/Contents/ContentItems/{TestOrder}");
                context.Missing(By.XPath(CompletePaymentButtonXPath));
            },
            browser);

    private static Task ClickDeleteItemAsync(UITestContext context, int index) =>
        context.ClickReliablyOnAsync(By.XPath($"//button[contains(@class, 'btn btn-danger')][{index.ToString(CultureInfo.InvariantCulture)}]"));

    private static Task ClickAddItemAsync(UITestContext context) => context.ClickReliablyOnAsync(By.Id("addButton"));

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
