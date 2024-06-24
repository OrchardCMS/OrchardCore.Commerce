using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.Settings;

namespace OrchardCore.Commerce.Tests.Fakes;

public class TestMoneyService : MoneyService
{
    public TestMoneyService()
        : base(
            new ICurrencyProvider[]
            {
                new CurrencyProvider(),
                new AnkhMorporkCurrencyProvider(), // #spell-check-ignore-line
            },
            new TestOptions<CurrencySettings>(new() { DefaultCurrency = "EUR" }),
            [new NullCurrencySelector()])
    {
    }
}
