using Atata;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using OrchardCore.Commerce.MoneyDataType;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OrchardCore.Commerce.Tests.UI.Tests.TieredPricePartTests;

public class PersistenceTieredPriceTests : UITestBase
{
    public PersistenceTieredPriceTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Theory, Chrome]
    public Task CreatingNewTieredPriceShouldPersist(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                await context.SignInDirectlyAsync();
                await context.CreateNewContentItemAsync("TestTieredPriceProduct");

                const string sku = "UITESTSKU";
                const string price = "9999";
                const string quantity1 = "2";
                const string unitPrice1 = "8999";
                const string quantity2 = "5";
                const string unitPrice2 = "7999";
                const string addPriceTierButtonClass = "add-price-tier-button";
                const string quantityInputXPath = "//input[contains(@class, 'tier-quantity-editor')]";
                const string unitPriceInputXPath = "//input[contains(@class, 'tier-unit-price-editor')]";
                var currency = Currency.HungarianForint.CurrencyIsoCode;

                await context.ClickAndFillInWithRetriesAsync(By.Id($"ProductPart_Sku"), sku);
                await context.ClickAndFillInWithRetriesAsync(By.Id("TieredPricePart_DefaultPrice"), price);
                await context.SetDropdownByTextAsync("TieredPricePart_Currency", currency);

                await context.ClickReliablyOnAsync(By.ClassName(addPriceTierButtonClass));
                await context.ClickAndFillInWithRetriesAsync(By.XPath($"({quantityInputXPath})[1]"), quantity1);
                await context.ClickAndFillInWithRetriesAsync(By.XPath($"({unitPriceInputXPath})[1]"), unitPrice1);
                await context.ClickReliablyOnAsync(By.ClassName(addPriceTierButtonClass));
                await context.ClickAndFillInWithRetriesAsync(By.XPath($"({quantityInputXPath})[2]"), quantity2);
                await context.ClickAndFillInWithRetriesAsync(By.XPath($"({unitPriceInputXPath})[2]"), unitPrice2);

                await context.ClickReliablyOnSubmitAsync();
                context.ShouldBeSuccess();

                await context.GoToContentItemListAsync("TestTieredPriceProduct");
                await context.ClickReliablyOnAsync(By.LinkText("Edit"));

                context.Get(By.Id("ProductPart_Sku")).GetDomProperty("value").ShouldBe(sku);

                context.Get(By.Id("TieredPricePart_DefaultPrice")).GetDomProperty("value").ShouldBe("9999.00");
                context.Get(By.CssSelector("#TieredPricePart_Currency option:checked").OfAnyVisibility())
                    .Text
                    .ShouldBe(currency);
                context.Get(By.XPath($"({quantityInputXPath})[1]")).GetDomProperty("value").ShouldBe(quantity1);
                context.Get(By.XPath($"({unitPriceInputXPath})[1]")).GetDomProperty("value").ShouldBe(unitPrice1);
                context.Get(By.XPath($"({quantityInputXPath})[2]")).GetDomProperty("value").ShouldBe(quantity2);
                context.Get(By.XPath($"({unitPriceInputXPath})[2]")).GetDomProperty("value").ShouldBe(unitPrice2);
            },
            browser);
}
