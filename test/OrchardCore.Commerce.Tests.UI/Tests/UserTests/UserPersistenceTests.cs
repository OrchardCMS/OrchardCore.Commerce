using Atata;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OrchardCore.Commerce.AddressDataType;
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

    private const string PhoneNumber = "(024) 2147878 ";
    private const string VatNumber = "BE09999999XX";

    public UserPersistenceTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Theory, Chrome]
    public Task UserAddressEditorShouldPersist(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                context.Missing(By.ClassName("user-addresses-widget"));
                await context.SignInDirectlyAndGoToHomepageAsync();
                await context.ClickReliablyOnAsync(By.ClassName("user-addresses-widget"));

                await context.FillAddressAsync(
                    "UserAddressesPart_ShippingAddress_Address_",
                    new Address
                    {
                        Name = ShippingName,
                        Department = ShippingDepartment,
                        StreetAddress1 = ShippingAddress,
                        City = ShippingCity,
                        PostalCode = ShippingPostalCode,
                        Region = ShippingCountryCode,
                    });

                await context.FillAddressAsync(
                    "UserAddressesPart_BillingAddress_Address_",
                    new Address
                    {
                        Name = BillingName,
                        Department = BillingDepartment,
                        Company = BillingCompany,
                        StreetAddress1 = BillingAddress,
                        City = BillingCity,
                        PostalCode = BillingPostalCode,
                        Region = BillingCountryCode,
                        Province = BillingStateCode,
                    });

                await context.ClickReliablyOnSubmitAsync();
                context.ShouldBeSuccess("Your addresses have been updated.");

                const string getInputsScript = @"return JSON.stringify(
                    Array.from(document.querySelectorAll(`form[action='/user/addresses'] input, form[action='/user/addresses'] select`))
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

    [Theory, Chrome]
    public Task UserDetailsEditorShouldPersist(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                await context.SignInDirectlyAsync();
                await context.GoToRelativeUrlAsync("/user/details");

                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("UserDetailsPart_PhoneNumber_Text"),
                    PhoneNumber);
                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("UserDetailsPart_VatNumber_Text"),
                    VatNumber);
                await context.ClickReliablyOnAsync(By.XPath("//label[contains(., 'User is a corporation')]"));

                await context.ClickReliablyOnSubmitAsync();
                context.ShouldBeSuccess("Your details have been updated.");

                const string getInputsScript = @"return JSON.stringify(
                    Array.from(document.querySelectorAll(`form[action='/user/details'] input, form[action='/user/details'] select`))
                        .map((element) => element.value))";
                var inputs = JsonConvert.DeserializeObject<string[]>(
                        context.ExecuteScript(getInputsScript).ToString()!);
                inputs.ShouldNotBeNull();
                inputs
                    .Take(3)
                    .ToArray()
                    .ShouldBe(new[]
                    {
                        PhoneNumber,
                        VatNumber,
                        "true",
                    });
            },
            browser);

    [Theory, Chrome]
    public Task AdminUserEditorShouldSaveWithoutFullyFilledUserAddresses(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                await context.SignInDirectlyAsync();
                await context.GoToRelativeUrlAsync("/Admin/Users/Create");

                await context.ClickAndFillInWithRetriesAsync(By.Id("User_UserName"), "TestUser");
                await context.ClickAndFillInWithRetriesAsync(By.Id("User_Email"), "test@example.com");
                await context.ClickReliablyOnAsync(By.ClassName("password-generator-button"));

                await SubmitAndGoToUserTabAsync(context, "User Addresses");

                const string testCustomerName = "Test Customer Name";
                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("User_UserAddressesPart_BillingAddress_Address_Name"),
                    testCustomerName);
                await context.SetCheckboxValueAsync(
                    By.Id("User_UserAddressesPart_BillingAndShippingAddressesMatch_Value"),
                    isChecked: true);

                await SubmitAndGoToUserTabAsync(context, "User Addresses");

                context
                    .Get(By.Id("User_UserAddressesPart_ShippingAddress_Address_Name").OfAnyVisibility())
                    .GetAttribute("value")?
                    .Trim()
                    .ShouldBe(testCustomerName);
            },
            browser);

    [Theory, Chrome]
    public Task PartiallyFilledUserDetailsShouldPersistInAdminUserEditor(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                await context.SignInDirectlyAsync();
                await context.GoToRelativeUrlAsync("/Admin/Users/Create");

                await context.ClickAndFillInWithRetriesAsync(By.Id("User_UserName"), "TestUser");
                await context.ClickAndFillInWithRetriesAsync(By.Id("User_Email"), "test@example.com");
                await context.ClickReliablyOnAsync(By.ClassName("password-generator-button"));

                await SubmitAndGoToUserTabAsync(context, "User Details");

                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("User_UserDetailsPart_PhoneNumber_Text"),
                    PhoneNumber);
                await context.ClickAndFillInWithRetriesAsync(
                    By.Id("User_UserDetailsPart_VatNumber_Text"),
                    VatNumber);

                await SubmitAndGoToUserTabAsync(context, "User Details");

                context
                    .Get(By.Id("User_UserDetailsPart_PhoneNumber_Text").OfAnyVisibility())
                    .GetAttribute("value")?
                    .ShouldBe(PhoneNumber);

                context
                    .Get(By.Id("User_UserDetailsPart_VatNumber_Text").OfAnyVisibility())
                    .GetAttribute("value")?
                    .Trim()
                    .ShouldBe(VatNumber);
            },
            browser);

    private static async Task SubmitAndGoToUserTabAsync(UITestContext context, string tabName)
    {
        await context.ClickReliablyOnSubmitAsync();
        context.ShouldBeSuccess();

        await context.ClickReliablyOnAsync(By.XPath(
            "//li[contains(@class, 'list-group-item')][.//h5[contains(., 'TestUser')]]" +
            "//a[contains(@class, 'btn-primary') and contains(., 'Edit')]"));
        await context.GoToEditorTabAsync(tabName);
    }
}
