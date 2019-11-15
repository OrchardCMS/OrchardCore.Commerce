using Money;
using Money.Abstractions;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.Settings;

namespace OrchardCore.Commerce.Tests.Fakes
{
    public class TestMoneyService : MoneyService
    {
        public TestMoneyService()
            : base(new ICurrencyProvider[]
            {
                new CurrencyProvider(),
                new AnkhMorporkCurrencyProvider()
            },
            new TestOptions<CommerceSettings>(new CommerceSettings { DefaultCurrency = "EUR" }))
        { }
    }
}
