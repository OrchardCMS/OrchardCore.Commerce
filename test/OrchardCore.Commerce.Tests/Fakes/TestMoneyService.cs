using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.Settings;

namespace OrchardCore.Commerce.Tests.Fakes;

public class TestMoneyService : MoneyService
{
    public TestMoneyService()
        : base(
            [
                new CurrencyProvider(),
                new AnkhMorporkCurrencyProvider(),
            ],
            new TestOptions<CurrencySettings>(new() { DefaultCurrency = "EUR" }),
            [new NullCurrencySelector()])
    {
    }
}
