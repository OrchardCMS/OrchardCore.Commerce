using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.MoneyDataType.Abstractions;

namespace OrchardCore.Commerce.Services;

public class NullCurrencySelector : ICurrencySelector
{
    public ICurrency CurrentDisplayCurrency => null;
}
