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
    private const string ShippingName = "Recipient";
    private const string ShippingDepartment = "Recipient Department";
    private const string ShippingAddress = "Test str. 1.";
    private const string ShippingCity = "Budapest";
    private const string ShippingPostalCode = "1234";
    private const string ShippingCountryCode = "HU";

    private const string BillingName = "Accountant";
    private const string BillingDepartment = "Accounting";
    private const string BillingCompany = "Recipient's Company";
    private const string BillingAddress = "Test str. 1.";
    private const string BillingCity = "Budapest";
    private const string BillingPostalCode = "30110";
    private const string BillingCountryCode = "US";
    private const string BillingStateCode = "GA";

    public UserPersistenceTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Theory, Chrome]
    public Task UserAddressEditorSholdPersist(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                await context.SignInDirectlyAsync();
                await context.GoToRelativeUrlAsync("/user/addresses");

                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("UserAddressesPart_ShippingAddress_Address_Name"),
                    ShippingName);
                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("UserAddressesPart_ShippingAddress_Address_Department"),
                    ShippingDepartment);
                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("UserAddressesPart_ShippingAddress_Address_StreetAddress1"),
                    ShippingAddress);
                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("UserAddressesPart_ShippingAddress_Address_City"),
                    ShippingCity);
                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("UserAddressesPart_ShippingAddress_Address_PostalCode"),
                    ShippingPostalCode);
                await context.SetDropdownByValueAsync(By.Id("UserAddressesPart_ShippingAddress_Address_Region"), ShippingCountryCode);

                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("UserAddressesPart_BillingAddress_Address_Name"),
                    BillingName);
                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("UserAddressesPart_BillingAddress_Address_Department"),
                    BillingDepartment);
                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("UserAddressesPart_BillingAddress_Address_Company"),
                    BillingCompany);
                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("UserAddressesPart_BillingAddress_Address_StreetAddress1"),
                    BillingAddress);
                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("UserAddressesPart_BillingAddress_Address_City"),
                    BillingCity);
                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("UserAddressesPart_BillingAddress_Address_PostalCode"),
                    BillingPostalCode);
                await context.SetDropdownByValueAsync(By.Id("UserAddressesPart_BillingAddress_Address_Region"), BillingCountryCode);
                await context.SetDropdownByValueAsync(By.Id("UserAddressesPart_BillingAddress_Address_Province"), BillingStateCode);

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
                        BillingName,
                        BillingDepartment,
                        BillingCompany,
                        BillingAddress,
                        string.Empty,
                        BillingCity,
                        BillingStateCode,
                        BillingPostalCode,
                        BillingCountryCode,
                        ShippingName,
                        ShippingDepartment,
                        string.Empty,
                        ShippingAddress,
                        string.Empty,
                        ShippingCity,
                        string.Empty,
                        ShippingPostalCode,
                        ShippingCountryCode,
                    });
            },
            browser);
}
