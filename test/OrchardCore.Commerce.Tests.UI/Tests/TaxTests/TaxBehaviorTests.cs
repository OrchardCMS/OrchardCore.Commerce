using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using OrchardCore.Commerce.AddressDataType;
using Shouldly;
using System.Globalization;
using Xunit;

using static OrchardCore.Commerce.Tests.UI.Constants.ContentItemIds;

namespace OrchardCore.Commerce.Tests.UI.Tests.TaxTests;

public class TaxBehaviorTests : UITestBase
{
    public const string NetPriceId = "PricePart_PriceField_Value";
    public const string GrossPriceId = "TaxPart_GrossPrice_Value";
    public const string TaxRateId = "TaxPart_TaxRate_Value";

    public TaxBehaviorTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Theory, Chrome]
    public Task TaxPartShouldUpdatePrice(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                await context.SignInDirectlyAsync();

                await context.EnableFeatureDirectlyAsync("OrchardCore.Localization");
                await context.GoToRelativeUrlAsync("/Admin/Settings/localization");
                await context.ClickReliablyOnAsync(By.XPath("//a[contains(., 'Add culture')]"));
                await context.ClickReliablyOnAsync(By.CssSelector("a[title='Remove culture']"));
                await context.ClickReliablyOnAsync(By.ClassName("primaryAction"));

                await context.GoToContentItemEditorByIdAsync(TestProduct);

                await context.ClickReliablyOnAsync(By.CssSelector("input[id^='priceField__switch_']"));
                await context.FillInWithRetriesAsync(By.Id(GrossPriceId), 10.ToTechnicalString());
                await context.FillInWithRetriesAsync(By.Id(TaxRateId), 25.ToTechnicalString());

                await context.ClickReliablyOnSubmitAsync();
                context.ShouldBeSuccess();

                FieldShouldBe(context, NetPriceId, 8);
                FieldShouldBe(context, GrossPriceId, 10);
                FieldShouldBe(context, TaxRateId, 25);
            },
            browser);

    [Theory, Chrome]
    public Task GrossPriceAndTaxRateShouldBeRequiredTogether(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                const string error = "You must either provide both Gross Price and Tax Rate, or neither of them.";

                await context.SignInDirectlyAsync();
                await context.GoToContentItemEditorByIdAsync(TestProduct);

                await context.FillInWithRetriesAsync(By.Id(TaxRateId), 25.ToTechnicalString());

                await context.ClickReliablyOnSubmitAsync();
                context.Exists(By.XPath(
                    $"//div[contains(@class, 'validation-summary-errors')]//li[contains(., '{error}')]"));
            },
            browser);

    [Theory, Chrome]
    public Task ProductDetailsShouldShowCorrectTaxRates(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                await context.SignInDirectlyAsync();
                await context.ExecuteRecipeDirectlyAsync("OrchardCore.Commerce.Samples.CustomTaxRates");

                var selector = By.CssSelector(".price-part-price-field-value, .tax-rate-gross-price-value");
                async Task VerifyPriceAsync(string expectedPrice)
                {
                    await context.GoToContentItemByIdAsync(TestProduct);
                    context.Exists(selector);
                    context.GetAll(selector)[^1].Text.Trim().ShouldBe(expectedPrice);
                }

                async Task UpdateAddressAndVerifyPriceAsync(Func<Task> configure, string expectedPrice)
                {
                    await context.GoToRelativeUrlAsync("/user/addresses");
                    await configure();
                    await context.ClickReliablyOnSubmitAsync();
                    await VerifyPriceAsync(expectedPrice);
                }

                const string prefix = "UserAddressesPart_BillingAddress_Address_";

                await VerifyPriceAsync("$5.00");

                await UpdateAddressAndVerifyPriceAsync(
                    async () =>
                    {
                        await context.SetCheckboxValueAsync(By.Id("UserAddressesPart_BillingAndShippingAddressesMatch_Value"), isChecked: true);
                        await context.FillAddressAsync(prefix, new Address
                        {
                            Name = "Test Customer",
                            StreetAddress1 = "Test Address",
                            City = "Test City",
                            Region = "HU",
                        });
                    },
                    "$6.25");

                await UpdateAddressAndVerifyPriceAsync(
                    () => context.FillAddressAsync(prefix, new Address { Region = "US", Province = "NY" }),
                    "$5.20");

                await UpdateAddressAndVerifyPriceAsync(
                    () => context.FillAddressAsync(prefix, new Address { Province = "NJ" }),
                    "$5.33");
            },
            browser);

    private static void FieldShouldBe(UITestContext context, string id, decimal value) =>
        decimal
            .Parse(context.Get(By.Id(id)).GetAttribute("value").Replace(',', '.'), CultureInfo.InvariantCulture)
            .ShouldBe(value);
}
