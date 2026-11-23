using Atata;
using Lombiq.Tests.UI.Extensions;
using OpenQA.Selenium;
using OrchardCore.Commerce.MoneyDataType;
using Shouldly;
using Xunit;

namespace OrchardCore.Commerce.Tests.UI.Tests.PriceVariantsPartTests;

public class PersistencePriceVariantsTests : UITestBase
{
    public PersistencePriceVariantsTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Fact]
    public Task CreatingNewPriceVariantShouldPersist() =>
        RunAsync(
            async context =>
            {
                await context.SignInDirectlyAsync();
                await context.CreateNewContentItemAsync("TestPriceVariantsProduct");

                const string sku = "UITESTSKU";
                const string price = "9999";
                var currency = Currency.HungarianForint.CurrencyIsoCode;

                await context.ClickAndFillInWithRetriesAsync(By.Id("ProductPart_Sku"), sku);
                await context.ClickAndFillInWithRetriesAsync(By.Id("PriceVariantsPart_VariantsValues__"), price);
                await context.SetDropdownByTextAsync("PriceVariantsPart_VariantsCurrencies__", currency);

                await context.ClickReliablyOnSubmitAsync();
                context.ShouldBeSuccess();

                await context.GoToContentItemListAsync("TestPriceVariantsProduct");
                await context.ClickReliablyOnAsync(By.XPath("//a[. = 'Edit']"));

                context.Get(By.Id("ProductPart_Sku")).GetDomProperty("value").ShouldBe(sku);

                context.Get(By.CssSelector("#PriceVariantsPart_VariantsCurrencies__ option:checked")
                        .OfAnyVisibility())
                    .Text
                    .ShouldBe(currency);

                context.Get(By.Id("PriceVariantsPart_VariantsValues__")).GetDomProperty("value").ShouldBe("9999.00");
            });

    [Fact]
    public Task AlteringPriceVariantShouldWarn() =>
        RunAsync(async context =>
        {
            await context.SignInDirectlyAsync();
            await context.GoToContentTypeEditorAsync(Constants.ContentTypes.PriceVariantsProduct);
            await context.ClickReliablyOnAsync(By.LinkText("Add Field"));

            await context.ClickAndFillInWithRetriesAsync(By.Id("DisplayName"), "Color");
            await context.ClickReliablyOnAsync(By.CssSelector("input[value='TextProductAttributeField']"));
            await context.ClickReliablyOnAsync(By.CssSelector("button[value='SaveAndConfigure']"));
            await context.ClickAndFillInWithRetriesAsync(
                By.Id("PriceVariantsProduct_Color_TextProductAttributeFieldSettingsDriver_PredefinedValues"),
                "Red\nGreen\nBlue");
            await context.ClickReliablyOnAsync(
                By.Id("PriceVariantsProduct_Color_TextProductAttributeFieldSettingsDriver_RestrictToPredefinedValues"));
            await Should.ThrowAsync<UnhandledAlertException>(() => context.ClickReliablyOnSubmitAsync());
            context.AcceptAlert();
            await context.ClickReliablyOnSubmitAsync();

            await context.GoToContentItemListAsync(Constants.ContentTypes.PriceVariantsProduct);
            var viewAbsoluteUri = new Uri(context.ExecuteScript("return document.querySelector('.view').href;").ToString());
            await context.ClickReliablyOnAsync(By.ClassName("edit"));
            await context.ClickReliablyOnSubmitAsync();
            context.ErrorMessageExists("PRICES ARE MISSING");

            context
                .GetAllWhenOneExists(By.CssSelector(".content-part-wrapper-price-variants-part .variant-price-value"))
                .Select((element, index) => new { Element = element, Value = 10 + index })
                .ForEach(pair => pair.Element.FillInWith(pair.Value.ToTechnicalString()));
            await Enumerable
                .Range(1, 9)
                .AwaitEachAsync(number => context.SetDropdownByValueAsync(
                    By.XPath($"(//select[starts-with(@name, 'PriceVariantsPart.VariantsCurrencies')])[{number.ToTechnicalString()}]"),
                    "USD"));
            await context.ClickReliablyOnSubmitAsync();
            context.ShouldBeSuccess();

            await context.GoToAbsoluteUrlAsync(viewAbsoluteUri);
            // Validate presence of new price variants and not the old price variants.
        });
}
