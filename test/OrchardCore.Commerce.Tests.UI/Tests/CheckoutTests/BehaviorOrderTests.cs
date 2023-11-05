using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentFields.Fields;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
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

                await context.FillCheckoutFormAsync(new OrderPart
                {
                    Email = new TextField { Text = "admin@example.com" },
                    Phone = new TextField { Text = "1234567" },
                    BillingAddress = new AddressField
                    {
                        Address = new Address
                        {
                            Name = "Gordon Freeman",
                            StreetAddress1 = "Black Mesa East 1.",
                            City = "City 17",
                        },
                        UserAddressToSave = nameof(OrderPart.BillingAddress),
                    },
                    BillingAndShippingAddressesMatch = new BooleanField { Value = true },
                });

                await context.ClickReliablyOnAsync(By.ClassName("pay-button-dummy"));

                void MatchText(string css, string expected) =>
                    context.Get(By.CssSelector(css)).Text.Trim().ShouldBe(expected);

                MatchText("h4.text-success", "Thank you for your purchase!");

                const string address = "";
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
}
