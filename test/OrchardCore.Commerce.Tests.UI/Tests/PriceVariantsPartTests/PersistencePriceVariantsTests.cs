using Atata;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
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
                await context.ClickAndFillInWithRetriesAsync(By.Id("PriceVariantsPart_VariantsValues_PRIMARY_"), price);
                await context.SetDropdownByTextAsync("PriceVariantsPart_VariantsCurrencies_PRIMARY_", currency);

                await context.ClickReliablyOnSubmitAsync();
                context.ShouldBeSuccess();

                await context.GoToContentItemListAsync("TestPriceVariantsProduct");
                await context.ClickReliablyOnAsync(By.ClassName("edit"));

                context.Get(By.Id("ProductPart_Sku")).GetDomProperty("value").ShouldBe(sku);

                context.Get(By.CssSelector("#PriceVariantsPart_VariantsCurrencies_PRIMARY_ option:checked")
                        .OfAnyVisibility())
                    .Text
                    .ShouldBe(currency);

                context.Get(By.Id("PriceVariantsPart_VariantsValues_PRIMARY_")).GetDomProperty("value").ShouldBe("9999.00");
            });

    [Fact]
    public Task AlteringPriceVariantShouldWarn() =>
        RunAsync(async context =>
        {
            var color = "Red Green Blue".Split();
            var size = "Small Medium Large".Split();
            var variants = color.CartesianProduct(size).Select(pair => $"{pair.Left}-{pair.Right}").ToList();
            var variantsUpper = variants.Select(value => value.ToUpperInvariant()).ToList();
            var byPriceEditors1 = GetPriceEditorSelectors(size.Select(value => value.ToUpperInvariant()));
            var byPriceEditors2 = GetPriceEditorSelectors(variantsUpper);
            var byCurrencyEditors = GetCurrencyEditorSelectors(variantsUpper);
            var byInventoryEditors = GetInventoryEditorSelectors(variantsUpper);

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
            // Can't use "ClickAndFillInWithRetriesAsync" because the newlines don't match on Windows.
            context.Driver.TryFillElement(
                context.Get(By.Id("PriceVariantsProduct_Color_TextProductAttributeFieldSettingsDriver_PredefinedValues")),
                string.Join(Keys.Enter, "Red", "Green", "Blue"));
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
            await byInventoryEditors
                .Select((by, index) => (By: by, Index: index))
                .AwaitEachAsync(pair => context.ClickAndFillInWithRetriesAsync(pair.By, (20 + pair.Index).ToTechnicalString()));
            byCurrencyEditors.Select(context.Get).ForEach(element => element.GetValue().ShouldBe("USD"));
            context.Exists(By.XPath("//h1[contains(., 'altered')]"));
            await context.ClickReliablyOnSubmitAsync();
            context.ShouldBeSuccess();

            await context.GoToAbsoluteUrlAsync(viewAbsoluteUri);
            ValidateLines(
                context,
                ".product-price-variants li",
                "RED-SMALL - $10.00",
                "RED-MEDIUM - $11.00",
                "RED-LARGE - $12.00",
                "GREEN-SMALL - $13.00",
                "GREEN-MEDIUM - $14.00",
                "GREEN-LARGE - $15.00",
                "BLUE-SMALL - $16.00",
                "BLUE-MEDIUM - $17.00",
                "BLUE-LARGE - $18.00");
            ValidateLines(
                context,
                ".field-name-inventory-part-inventory .d-block",
                "RED-SMALL Inventory: 20",
                "RED-MEDIUM Inventory: 21",
                "RED-LARGE Inventory: 22",
                "GREEN-SMALL Inventory: 23",
                "GREEN-MEDIUM Inventory: 24",
                "GREEN-LARGE Inventory: 25",
                "BLUE-SMALL Inventory: 26",
                "BLUE-MEDIUM Inventory: 27",
                "BLUE-LARGE Inventory: 28");
            ValidateLines(
                context,
                ".product-attribute-list label",
                "Size",
                "Small",
                "Medium",
                "Large",
                "Color",
                "Red",
                "Green",
                "Blue");
        });

    private static List<By> GetSelectors(IEnumerable<string> variants, string idPrefix) =>
        variants
            .Select(variant => By.Id($"{idPrefix}{variant}_"))
            .ToList();

    private static List<By> GetPriceEditorSelectors(IEnumerable<string> variants) =>
        GetSelectors(variants, "PriceVariantsPart_VariantsValues_");

    private static List<By> GetCurrencyEditorSelectors(IEnumerable<string> variants) =>
        GetSelectors(variants, "PriceVariantsPart_VariantsCurrencies_");

    private static List<By> GetInventoryEditorSelectors(IEnumerable<string> variants) =>
        GetSelectors(variants, "InventoryPart_Inventory_TESTPRODUCTVARIANT-");

    private static void ValidateLines(UITestContext context, string css, params string[] expected) =>
        context.GetAllWhenOneExists(By.CssSelector(css))
            .Select(element => element.GetTextTrimmed())
            .ToArray()
            .ShouldBe(expected);
}
