using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Helpers;
using OpenQA.Selenium;
using OrchardCore.Commerce.Abstractions.Fields;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.ContentFields.Fields;

namespace Lombiq.Tests.UI.Services;

public static class FormUITestContextExtensions
{
    public static Task ClickCheckoutAsync(this UITestContext context) =>
        context.ClickReliablyOnAsync(By.ClassName("checkout"));

    public static async Task FillAddressAsync(this UITestContext context, string prefix, Address address)
    {
        if (address is null) return;

        Task FillAsync(string suffix, string value, bool isDropdown = false)
        {
            var by = By.Id(prefix + suffix);
            if (value is null || context.Get(by).GetAttribute("value") == value) return Task.CompletedTask;

            if (!isDropdown)
            {
                return context.ClickAndFillInWithRetriesAsync(by, value);
            }

            return ReliabilityHelper.DoWithRetriesOrFailAsync(async () =>
            {
                try
                {
                    await context.SetDropdownByValueAsync(by, value);
                    return true;
                }
                catch (WebDriverException)
                {
                    return false;
                }
            });
        }

        await FillAsync(nameof(address.Name), address.Name);
        await FillAsync(nameof(address.Department), address.Department);
        await FillAsync(nameof(address.Company), address.Company);
        await FillAsync(nameof(address.StreetAddress1), address.StreetAddress1);
        await FillAsync(nameof(address.StreetAddress2), address.StreetAddress2);
        await FillAsync(nameof(address.City), address.City);
        await FillAsync(nameof(address.PostalCode), address.PostalCode);
        await FillAsync(nameof(address.Region), address.Region, isDropdown: true);
        await FillAsync(nameof(address.Province), address.Province, isDropdown: true);
    }

    public static async Task FillAddressAsync(this UITestContext context, string fieldName, AddressField field)
    {
        if (field is null) return;

        await context.FillAddressAsync(
            $"{nameof(OrderPart)}_{fieldName}_{nameof(field.Address)}_",
            field.Address);

        await context.SetCheckboxValueAsync(
            By.Id($"{nameof(OrderPart)}_{fieldName}_ToBeSaved"),
            field.UserAddressToSave == fieldName);
    }

    public static async Task FillCheckoutFormAsync(this UITestContext context, OrderPart data)
    {
        if (data is null) return;

        async Task FillTextFieldAsync(string fieldName, TextField field)
        {
            var by = By.Id($"{nameof(OrderPart)}_{fieldName}_{nameof(field.Text)}");
            if (field?.Text is not { } text || context.Get(by).GetAttribute("value") == text) return;
            await context.ClickAndFillInWithRetriesAsync(by, text);
        }

        Task FillBooleanFieldAsync(string fieldName, BooleanField field) =>
            field is null
                ? Task.CompletedTask
                : context.SetCheckboxValueAsync(By.Id($"{nameof(OrderPart)}_{fieldName}_{nameof(field.Value)}"), field.Value);

        await FillTextFieldAsync(nameof(data.Email), data.Email);
        await FillTextFieldAsync(nameof(data.Phone), data.Phone);
        await FillTextFieldAsync(nameof(data.VatNumber), data.VatNumber);
        await FillBooleanFieldAsync(nameof(data.IsCorporation), data.IsCorporation);

        await context.FillAddressAsync(nameof(data.BillingAddress), data.BillingAddress);

        var sameAddress = data.BillingAndShippingAddressesMatch;
        await FillBooleanFieldAsync(nameof(data.BillingAndShippingAddressesMatch), sameAddress);
        if (!sameAddress.Value) await context.FillAddressAsync(nameof(data.ShippingAddress), data.ShippingAddress);
    }
}
