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
            var color = "RED GREEN BLUE".Split();
            var size = "SMALL MEDIUM LARGE".Split();
            var variants = color.CartesianProduct(size).Select(pair => $"{pair.Left}-{pair.Right}").ToList();
            var byPriceEditors1 = GetPriceEditorSelectors(size);
            var byPriceEditors2 = GetPriceEditorSelectors(variants);

            await context.SignInDirectlyAsync();

            await context.GoToContentItemListAsync(Constants.ContentTypes.PriceVariantsProduct);
            await context.ClickReliablyOnAsync(By.ClassName("edit"));
            byPriceEditors1.ForEach(by => context.Exists(by));
            byPriceEditors2.ForEach(by => context.Missing(by));

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
            byPriceEditors1.ForEach(by => context.Missing(by));
            byPriceEditors2.ForEach(by => context.Exists(by));

            // Validate that pressing the Publish button won't cause a navigation by HTML editing. (Can't directly
            // access the "Please fill out this field" pop-up so this is a necessary workaround.)
            context.ExecuteScript("document.getElementsByTagName('h1')[0].textContent = 'altered'");
            await context.ClickReliablyOnSubmitAsync();
            await byPriceEditors2
                .Select((by, index) => (By: by, Index: index))
                .AwaitEachAsync(pair => context.ClickAndFillInWithRetriesAsync(pair.By, (10 + pair.Index).ToTechnicalString()));
            GetCurrencyEditorSelectors(variants).Select(context.Get).ForEach(element => element.GetValue().ShouldBe("USD"));
            context.Exists(By.XPath("//h1[contains(., 'altered')]"));
            await context.ClickReliablyOnSubmitAsync();

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

    private static List<By> GetPriceEditorSelectors(IEnumerable<string> variants) =>
        variants
            .Select(variant => By.Id($"PriceVariantsPart_VariantsValues_{variant}_"))
            .ToList();

    private static List<By> GetCurrencyEditorSelectors(IEnumerable<string> variants) =>
        variants
            .Select(variant => By.Id($"PriceVariantsPart_VariantsCurrencies_{variant}_"))
            .ToList();
}
