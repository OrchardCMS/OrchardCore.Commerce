using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
using System.Globalization;
using Xunit;
using Xunit.Abstractions;
using static OrchardCore.Commerce.Tests.UI.Constants.ContentItemIds;

namespace OrchardCore.Commerce.Tests.UI.Tests.TaxTests;

public class TaxBehaviourTests : UITestBase
{
    public const string NetPriceId = "PricePart_PriceField_Value";
    public const string GrossPriceId = "TaxPart_GrossPrice_Value";
    public const string TaxRateId = "TaxPart_TaxRate_Value";

    public TaxBehaviourTests(ITestOutputHelper testOutputHelper)
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

    private static void FieldShouldBe(UITestContext context, string id, decimal value) =>
        decimal
            .Parse(
                context.Get(By.Id(id)).GetAttribute("value").Replace(',', '.'),
                CultureInfo.InvariantCulture)
            .ShouldBe(value);
}
