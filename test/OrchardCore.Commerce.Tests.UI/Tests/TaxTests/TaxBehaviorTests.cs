using Atata;
using GraphQL;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.Tax.Constants;
using OrchardCore.Commerce.Tax.Models;
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

    [Theory, Chrome]
    public Task CustomTaxRateEditorShouldWork(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                void ResetScroll() => context.ExecuteScript("window.scrollTo(0, 0);");

                By ByCell(int index, string name) =>
                    By.CssSelector($".taxRateSettings__row_{index.ToTechnicalString()} .taxRateSettings__{name.ToCamelCase()}");

                Task SetCellAsync(int index, string name, string value) =>
                    context.ClickAndFillInWithRetriesAsync(ByCell(index, name), value);

                Task SetCorporationAsync(int index, MatchTaxRates value) =>
                    context.SetDropdownByValueAsync(
                        ByCell(index, nameof(TaxRateSetting.IsCorporation)),
                        value.ToString());

                void SetTaxRate(int index, decimal value)
                {
                    // This is a workaround for https://github.com/Lombiq/UI-Testing-Toolbox/issues/628.
                    var element = context.Get(ByCell(index, nameof(TaxRateSetting.TaxRate)));
                    element.Click();
                    element.Clear();
                    element.SendKeysWithLogging(value.ToTechnicalString());
                }

                // Enable feature.
                await context.SignInDirectlyAsync();
                await context.EnableFeatureDirectlyAsync(FeatureIds.CustomTaxRates);

                // Verify the menu navigation.
                await context.GoToDashboardAsync();
                await context.ClickReliablyOnAsync(By.ClassName("menu-configuration"));
                await context.ClickReliablyOnAsync(By.XPath(
                    "//ul[@data-title='Configuration']/li[contains(@class, 'has-items') and " +
                    ".//span[contains(@class, 'title') and contains(., 'Commerce')]]"));
                await context.ClickReliablyOnAsync(By.LinkText("Custom Tax Rates"));
                context.Get(By.TagName("h2")).GetTextTrimmed().ShouldBe("Recipients and Tax Codes");

                // Fill out first row with example data.
                await SetCellAsync(0, nameof(TaxRateSetting.DestinationCity), "Saint-Nicéphore");
                await SetCellAsync(0, nameof(TaxRateSetting.DestinationProvince), "QC");
                await SetCellAsync(0, nameof(TaxRateSetting.DestinationPostalCode), "J2A 1R1");
                await SetCellAsync(0, nameof(TaxRateSetting.DestinationRegion), "CA[");
                await SetCellAsync(0, nameof(TaxRateSetting.TaxCode), "TVQ");
                await SetCellAsync(0, nameof(TaxRateSetting.VatNumber), "1");
                SetTaxRate(0, 0.01m);
                await SetCorporationAsync(0, MatchTaxRates.Checked);
                ResetScroll();

                // Add another row and fill it with another example.
                await context.ClickReliablyOnAsync(By.ClassName("taxRateSettings__addButton"));
                await SetCellAsync(1, nameof(TaxRateSetting.DestinationCity), "Budapest");
                await SetCellAsync(1, nameof(TaxRateSetting.DestinationPostalCode), "][");
                await SetCellAsync(1, nameof(TaxRateSetting.DestinationRegion), "HU");
                SetTaxRate(1, 27);
                ResetScroll();

                // Click "Save", error message should be displayed after page load.
                await context.ClickReliablyOnAsync(By.ClassName("save"));
                context.Exists(By.ClassName("validation-summary-errors"));

                // Verify that error message contents match expectations.
                context
                    .GetAll(By.CssSelector(".validation-summary-errors li"))
                    .Select(element => element.GetTextTrimmed())
                    .ToList()
                    .ShouldBe(
                    [
                        "The value in column \"Country or region code\" in row 1 must be empty or a valid regular expression.",
                        "The value in column \"Postal code\" in row 2 must be empty or a valid regular expression."
                    ]);

                // Fix incorrect cells and validate that the data is successfully retained.
                var byAllInputs = By.CssSelector(".taxRateSettings__row input");
                await SetCellAsync(0, nameof(TaxRateSetting.DestinationRegion), "CA");
                await SetCellAsync(1, nameof(TaxRateSetting.DestinationPostalCode), "^1[0-9][0-9][0-9]");
                await context.ClickReliablyOnAsync(By.ClassName("save"));
                context.ShouldBeSuccess("Site settings updated successfully.");
                await context.Driver.Navigate().RefreshAsync();
                context.Exists(byAllInputs);
                context
                    .GetAll(byAllInputs)
                    .Select(element => element.GetValue())
                    .ToList()
                    .ShouldBe(
                    [
                        "Saint-Nicéphore",
                        "QC",
                        "J2A 1R1",
                        "CA",
                        "TVQ",
                        "1",
                        "0.01",

                        "Budapest",
                        string.Empty,
                        "^1[0-9][0-9][0-9]",
                        "HU",
                        string.Empty,
                        string.Empty,
                        "27"]);
            },
            browser);

    private static void FieldShouldBe(UITestContext context, string id, decimal value) =>
        decimal
            .Parse(context.Get(By.Id(id)).GetAttribute("value")!.Replace(',', '.'), CultureInfo.InvariantCulture)
            .ShouldBe(value);
}
