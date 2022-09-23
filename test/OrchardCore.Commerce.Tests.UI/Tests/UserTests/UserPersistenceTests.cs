using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using Newtonsoft.Json;
using OpenQA.Selenium;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OrchardCore.Commerce.Tests.UI.Tests.UserTests;

public class UserPersistenceTests : UITestBase
{
    public UserPersistenceTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Theory, Chrome]
    public Task TaxPartShouldUpdatePrice(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                await context.SignInDirectlyAsync();
                await context.GoToRelativeUrlAsync("/user/addresses");

                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("UserAddressesPart_ShippingAddress_Address_Name"),
                    "Recipient");
                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("UserAddressesPart_ShippingAddress_Address_Department"),
                    "Recipient Department");
                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("UserAddressesPart_ShippingAddress_Address_StreetAddress1"),
                    "Test str. 1.");
                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("UserAddressesPart_ShippingAddress_Address_City"),
                    "Budapest");
                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("UserAddressesPart_ShippingAddress_Address_PostalCode"),
                    "1234");
                await context.SetDropdownByTextAsync("UserAddressesPart_ShippingAddress_Address_Region", "Magyarország");

                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("UserAddressesPart_BillingAddress_Address_Name"),
                    "Accountant");
                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("UserAddressesPart_BillingAddress_Address_Department"),
                    "Accounting");
                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("UserAddressesPart_BillingAddress_Address_Company"),
                    "Recipient's Company");
                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("UserAddressesPart_BillingAddress_Address_StreetAddress1"),
                    "Test str. 2.");
                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("UserAddressesPart_BillingAddress_Address_City"),
                    "Budapest");
                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("UserAddressesPart_BillingAddress_Address_PostalCode"),
                    "30110");
                await context.SetDropdownByTextAsync("UserAddressesPart_BillingAddress_Address_Region", "United States");
                await context.SetDropdownByTextAsync("UserAddressesPart_BillingAddress_Address_Province", "Georgia");

                await context.ClickReliablyOnSubmitAsync();
                context.ShouldBeSuccess("Your addresses have been updated.");

                const string getInputsScript = @"return JSON.stringify(
                    Array.from(document.querySelectorAll('form[action=\'/user/addresses\'] input, form[action=\'/user/addresses\'] select'))
                        .map((element) => element.value))";
                var inputs = JsonConvert.DeserializeObject<string[]>(
                        context.ExecuteScript(getInputsScript).ToString()!);
                inputs.ShouldNotBeNull();
                inputs
                    .Take(18)
                    .ToArray()
                    .ShouldBe(new[]
                    {
                        "Recipient",
                        "Recipient Department",
                        string.Empty,
                        "Test str. 1.",
                        string.Empty,
                        "Budapest",
                        string.Empty,
                        "1234",
                        "HU",
                        "Accountant",
                        "Accounting",
                        "Recipient's Company",
                        "Test str. 2.",
                        string.Empty,
                        "Budapest",
                        "GA",
                        "30110",
                        "US",
                    });
            },
            browser);
}
