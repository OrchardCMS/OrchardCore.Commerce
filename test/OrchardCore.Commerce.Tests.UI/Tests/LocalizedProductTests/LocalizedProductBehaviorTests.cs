using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using OrchardCore.Commerce.MoneyDataType;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OrchardCore.Commerce.Tests.UI.Tests.LocalizedProductTests;

public class LocalizedProductBehaviorTests : UITestBase
{
    private const string LocalizedTitle = "Honosított Termék"; // #spell-check-ignore-line

    private static readonly By _localizationsButtonPath = By.XPath(
        "//li[contains(@class, 'list-group-item') and .//a[contains(., 'Test Localized Product')]]//div[@title = 'Localizations']//button");
    private static readonly string _localizedCurrency = Currency.HungarianForint.CurrencyIsoCode;

    public LocalizedProductBehaviorTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Theory, Chrome]
    public Task LocalizedProductCreationShouldBeAllowed(Browser browser) =>
        ExecuteTestAfterSetupAsync(
            async context =>
            {
                await context.SignInDirectlyAsync();
                await GoToLocalizedProductAsync(context);

                var localizationsButton = context.Get(_localizationsButtonPath);
                await context.SelectFromBootstrapDropdownReliablyAsync(
                    localizationsButton,
                    By.XPath(".//a[contains(., 'hu-HU')]"));

                await context.ClickAndFillInWithRetriesAsync(By.Id("TitlePart_Title"), LocalizedTitle);
                await context.ClickAndFillInWithRetriesAsync(By.Id("PricePart_PriceField_Value"), "3500");
                await context.SetDropdownByTextAsync("PricePart_PriceField_Currency", _localizedCurrency);
                await context.ClickReliablyOnSubmitAsync();
                context.ShouldBeSuccess();

                await context.GoToAdminRelativeUrlAsync("/Settings/localization");
                await context.ClickReliablyOnAsync(By.CssSelector("input.form-check-input[value='hu-HU']"));
                await context.ClickReliablyOnAsync(By.ClassName("save"));

                await context.GoToAdminRelativeUrlAsync("/Settings/commerce");
                await context.SetDropdownByValueAsync(By.Id("ISite_CurrencySettings_CurrentDisplayCurrency"), _localizedCurrency);
                await context.ClickReliablyOnAsync(By.ClassName("save"));

                await GoToLocalizedProductAsync(context);
                await context.ClickReliablyOnAsync(By.LinkText("View"));
                context.SwitchToLastWindow();

                context.ErrorMessageShouldNotExist();
                context.Get(By.CssSelector("header.masthead h1")).Text.Trim().ShouldBe(LocalizedTitle);

                await context.ClickReliablyOnAsync(By.CssSelector("form[action='/shoppingcart/AddItem'] button.btn-primary"));
                context.ErrorMessageShouldNotExist();
                context.Get(By.ClassName("cart-product-name")).Text.Trim().ShouldBe(LocalizedTitle);
                context.Get(By.ClassName("shopping-cart-table-unit-price")).Text.Trim().ShouldBe("3 500,00 Ft");
            },
            browser);

    private static Task GoToLocalizedProductAsync(UITestContext context) =>
        context.GoToAdminRelativeUrlAsync("/Contents/ContentItems?q=Test%20Localized%20Product type%3ALocalizedProduct"); // #spell-check-ignore-line
}
