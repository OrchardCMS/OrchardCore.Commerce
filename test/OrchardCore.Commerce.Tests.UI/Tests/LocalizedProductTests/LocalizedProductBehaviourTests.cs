using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OrchardCore.Commerce.Tests.UI.Tests.LocalizedProductTests;

public class LocalizedProductBehaviourTests : UITestBase
{
    private const string LocalizedTitle = "Honosított Termék"; // #spell-check-ignore-line
    private const string LocalizationsButtonPath =
        "//li[contains(@class, 'list-group-item') and .//a[contains(., 'Test Localized Product')]]//div[@title = 'Localizations']//button";

    public LocalizedProductBehaviourTests(ITestOutputHelper testOutputHelper)
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

                var localizationsButton = context.Get(By.XPath(LocalizationsButtonPath));
                await context.SelectFromBootstrapDropdownReliablyAsync(
                    localizationsButton,
                    By.XPath(".//a[contains(., 'hu-HU')]"));

                await context.ClickAndFillInWithRetriesAsync(By.Id("TitlePart_Title"), LocalizedTitle);
                await context.ClickAndFillInWithRetriesAsync(By.Id("PricePart_PriceField_Value"), "3500");
                await context.SetDropdownByTextAsync("PricePart_PriceField_Currency", "HUF");
                await context.ClickReliablyOnSubmitAsync();
                context.ShouldBeSuccess();

                await context.GoToAdminRelativeUrlAsync("/Settings/localization");
                await context.ClickReliablyOnAsync(By.CssSelector("input.form-check-input[value='hu-HU']"));
                await context.ClickReliablyOnAsync(By.ClassName("save"));

                await context.GoToAdminRelativeUrlAsync("/Settings/commerce");
                await context.SetDropdownByValueAsync(By.Id("ISite_CurrentDisplayCurrency"), "HUF");
                await context.ClickReliablyOnAsync(By.ClassName("save"));

                await GoToLocalizedProductAsync(context);
                await context.ClickReliablyOnAsync(By.LinkText("View"));
                context.SwitchToLastWindow();

                context.Missing(By.ClassName("message-error"));
                context.Get(By.CssSelector("header.masthead h1")).Text.Trim().ShouldBe(LocalizedTitle);

                await context.ClickReliablyOnAsync(By.CssSelector("form[action='/shoppingcart/AddItem'] button.btn-primary"));
                context.Missing(By.ClassName("message-error"));
            },
            browser);

    private static Task GoToLocalizedProductAsync(UITestContext context) =>
        context.GoToAdminRelativeUrlAsync("/Contents/ContentItems?q=Test%20Localized%20Product type%3ALocalizedProduct");
}
