using System;
using Money;
using Money.Abstractions;
using Newtonsoft.Json;
using Xunit;

namespace OrchardCore.Commerce.Tests
{
    public class AmountTests
    {
        [Fact]
        public void CantConstructAnAmountWithNullCurrency()
        {
            Assert.Throws<ArgumentNullException>(() => new Amount(1, (ICurrency)null));
        }

        [Fact]
        public void AmountsCanBeAdded()
        {
            Assert.Equal(new Amount(42.23M, Currency.Euro), new Amount(21.12M, Currency.Euro) + new Amount(21.11M, Currency.Euro));
        }

        [Fact]
        public void AddingDifferentCurrenciesThrows()
        {
            Assert.Throws<InvalidOperationException>(() => new Amount(1, Currency.USDollar) + new Amount(1, Currency.Euro));
        }

        [Fact]
        public void AmountsCanBeSubtracted()
        {
            Assert.Equal(new Amount(0.01M, Currency.Euro), new Amount(21.12M, Currency.Euro) - new Amount(21.11M, Currency.Euro));
        }

        [Fact]
        public void SubtractingDifferentCurrenciesThrows()
        {
            Assert.Throws<InvalidOperationException>(() => new Amount(1, Currency.USDollar) - new Amount(1, Currency.Euro));
        }

        [Fact]
        public void AmountsCanBeNegated()
        {
            Assert.Equal(new Amount(-42, Currency.Euro), -new Amount(42, Currency.Euro));
            Assert.Equal(new Amount(42, Currency.Euro), -new Amount(-42, Currency.Euro));
        }

        [Fact]
        public void OneCanMultiplyAnAmountByANumber()
        {
            Assert.Equal(new Amount(42.34M, Currency.Euro), 2 * new Amount(21.17M, Currency.Euro));
            Assert.Equal(new Amount(42.34M, Currency.Euro), new Amount(21.17M, Currency.Euro) * 2);
            Assert.Equal(new Amount(42.34M, Currency.Euro), 2f * new Amount(21.17M, Currency.Euro));
            Assert.Equal(new Amount(42.34M, Currency.Euro), new Amount(21.17M, Currency.Euro) * 2f);
            Assert.Equal(new Amount(42.34M, Currency.Euro), 2d * new Amount(21.17M, Currency.Euro));
            Assert.Equal(new Amount(42.34M, Currency.Euro), new Amount(21.17M, Currency.Euro) * 2d);
            Assert.Equal(new Amount(42.34M, Currency.Euro), 2m * new Amount(21.17M, Currency.Euro));
            Assert.Equal(new Amount(42.34M, Currency.Euro), new Amount(21.17M, Currency.Euro) * 2m);
        }

        [Fact]
        public void AmountsOfTheSameCurrencyCanBeCompared()
        {
            Assert.Equal(0, new Amount(1, Currency.Euro).CompareTo(new Amount(1, Currency.Euro)));
            Assert.Equal(1, new Amount(2, Currency.Euro).CompareTo(new Amount(1, Currency.Euro)));
            Assert.Equal(-1, new Amount(1, Currency.Euro).CompareTo(new Amount(2, Currency.Euro)));
            Assert.True(new Amount(1, Currency.Euro) == new Amount(1, Currency.Euro));
            Assert.False(new Amount(1, Currency.Euro) == new Amount(2, Currency.Euro));
            Assert.True(new Amount(1, Currency.Euro) != new Amount(2, Currency.Euro));
            Assert.False(new Amount(1, Currency.Euro) != new Amount(1, Currency.Euro));
            Assert.True(new Amount(1, Currency.Euro) < new Amount(2, Currency.Euro));
            Assert.False(new Amount(1, Currency.Euro) < new Amount(1, Currency.Euro));
            Assert.False(new Amount(2, Currency.Euro) < new Amount(1, Currency.Euro));
            Assert.True(new Amount(2, Currency.Euro) > new Amount(1, Currency.Euro));
            Assert.False(new Amount(1, Currency.Euro) > new Amount(1, Currency.Euro));
            Assert.False(new Amount(1, Currency.Euro) > new Amount(2, Currency.Euro));
            Assert.True(new Amount(1, Currency.Euro) <= new Amount(2, Currency.Euro));
            Assert.True(new Amount(1, Currency.Euro) <= new Amount(1, Currency.Euro));
            Assert.False(new Amount(2, Currency.Euro) <= new Amount(1, Currency.Euro));
            Assert.True(new Amount(2, Currency.Euro) >= new Amount(1, Currency.Euro));
            Assert.True(new Amount(1, Currency.Euro) >= new Amount(1, Currency.Euro));
            Assert.False(new Amount(1, Currency.Euro) >= new Amount(2, Currency.Euro));
        }

        [Fact]
        public void ComparingAmountsOfDifferentCurrenciesThrows()
        {
            Assert.Throws<InvalidOperationException>(() => new Amount(1, Currency.Euro).CompareTo(new Amount(1, Currency.USDollar)));
            Assert.Throws<InvalidOperationException>(() => new Amount(1, Currency.Euro) < new Amount(2, Currency.USDollar));
            Assert.Throws<InvalidOperationException>(() => new Amount(2, Currency.Euro) > new Amount(1, Currency.USDollar));
            Assert.Throws<InvalidOperationException>(() => new Amount(1, Currency.Euro) <= new Amount(2, Currency.USDollar));
            Assert.Throws<InvalidOperationException>(() => new Amount(2, Currency.Euro) >= new Amount(1, Currency.USDollar));
        }

        [Fact]
        public void SerialisationPersistsValidValues()
        {
            var amt1 = new Amount(1.23M, Currency.Euro);

            var s = JsonConvert.SerializeObject(amt1);

            var amt2 = JsonConvert.DeserializeObject<Amount>(s);

            Assert.Equal(amt1, amt2);
        }

        [Fact]
        public void SerialisationPersistsUnknownCurrencies()
        {
            var unknown = new Currency("My FOO", "f", "FOO");

            var amt1 = new Amount(1.23M, unknown);

            var s = JsonConvert.SerializeObject(amt1);

            var amt2 = JsonConvert.DeserializeObject<Amount>(s);

            Assert.True(amt1 == amt2);

            Assert.Equal(amt1, amt2);
        }
    }
}
