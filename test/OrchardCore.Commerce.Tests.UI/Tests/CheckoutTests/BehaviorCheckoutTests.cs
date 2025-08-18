using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using OrchardCore.Commerce.Abstractions.Fields;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.ContentFields.Fields;
using Shouldly;
using Xunit;
using static OrchardCore.Commerce.ContentFields.Constants.FeatureIds;
using static OrchardCore.Commerce.Tests.UI.Constants.ContentItemIds;

namespace OrchardCore.Commerce.Tests.UI.Tests.OrderTests;

public class BehaviorCheckoutTests : UITestBase
{
    public BehaviorCheckoutTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Theory, Chrome]
    public Task DummyCheckoutShouldWork(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                await context.SignInDirectlyAsync();
                await context.GoToContentItemDisplayByIdAsync(TestProduct);

                await context.ClickAndFillInWithRetriesAsync(By.Id("product-TESTPRODUCT-quantity"), "2");
                await context.ClickReliablyOnSubmitAsync();
                await context.ClickCheckoutAsync();

                await FillCheckoutFormAsync(context);
                await context.ClickReliablyOnAsync(By.ClassName("pay-button-dummy"));

                void MatchText(string css, string expected) =>
                    context.Get(By.CssSelector(css)).Text.Trim().RegexReplace(@"\s+", " ").ShouldBe(expected);

                MatchText("h4.text-success", "Thank you for your purchase!");

                const string address = "BLACK MESA EAST 1. CITY 17 AF";
                MatchText(".field-name-order-part-billing-address dd", address);
                MatchText(".field-name-order-part-shipping-address dd", address);

                const string total = "$10.00";
                MatchText("#shopping-cart .order-total", total);

                var orderId = context.Driver.Url.RegexMatch(@".*\/success\/(.*)").Groups[1];
                MatchText(".order-charge-kind", "Dummy Payment");
                MatchText(".order-charge-amount", total);
                MatchText(".order-charge-transaction-id", $"{orderId}:0");
                MatchText(".order-charge-text", "Dummy transaction of US Dollar.");
            },
            browser);

    [Theory, Chrome]
    public Task AddressPartsFeatureShouldOverrideRegularCheckout(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                await context.SignInDirectlyAsync();
                await context.EnableFeatureDirectlyAsync(WesternNameParts);

                await context.GoToContentItemDisplayByIdAsync(TestProduct);
                await context.ClickReliablyOnSubmitAsync();
                await context.ClickCheckoutAsync();

                var noteBy = By.Name("OrderPart.ShippingAddress.Address.AdditionalFields.NoteForCourier");
                var noteText = "Don't take the old passage to Ravenholm. We don't go there anymore.";
                await context.ClickAndFillInWithRetriesAsync(noteBy, noteText);
                await context.ClickAndFillInWithRetriesAsync(AddressName("GivenName"), "Gordon");
                await context.ClickAndFillInWithRetriesAsync(AddressName("FamilyName"), "Freeman");
                await context.SetDropdownByValueAsync(AddressName("Honorific"), "Dr");
                await FillCheckoutFormAsync(context, includeName: false);

                await context.ClickReliablyOnAsync(By.ClassName("pay-button-dummy"));
                context.Get(By.CssSelector("h4.text-success"));

                await context.GoToContentItemListAsync();
                await context.ClickReliablyOnAsync(By.ClassName("edit"));
                context
                    .Get(By.Id("OrderPart_BillingAddress_Address_Name"))
                    .GetAttribute("value")
                    .ShouldBe("Dr Gordon Freeman");

                await context.ClickReliablyOnAsync(By.Id("OrderPart_BillingAndShippingAddressesMatch_Value"));
                context.Get(noteBy).GetAttribute("value").ShouldBe(noteText);
            },
            browser);

    private static By AddressName(string key) =>
        By.Name($"OrderPart.BillingAddress.Address.NameParts.{key}");

    private static Task FillCheckoutFormAsync(UITestContext context, bool includeName = true) =>
        context.FillCheckoutFormAsync(new OrderPart
        {
            Email = new TextField { Text = "admin@example.com" },
            Phone = new TextField { Text = "1234567" },
            BillingAddress = new AddressField
            {
                Address = new Address
                {
                    Name = includeName ? "Gordon Freeman" : null,
                    StreetAddress1 = "Black Mesa East 1.",
                    City = "City 17",
                },
                UserAddressToSave = nameof(OrderPart.BillingAddress),
            },
            BillingAndShippingAddressesMatch = new BooleanField { Value = true },
        });
}
