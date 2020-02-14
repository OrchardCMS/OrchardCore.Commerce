using System;
using Money.Abstractions;
using OrchardCore.Commerce.Abstractions;

namespace OrchardCore.Commerce.Services
{
    public class NullCurrencySelector : ICurrencySelector
    {
        public ICurrency CurrentDisplayCurrency => null;
    }
}
