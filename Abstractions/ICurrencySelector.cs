using Money.Abstractions;

namespace OrchardCore.Commerce.Abstractions
{
    /// <summary>
    /// Implementations of this interface can alter the currency used for showing prices to the customer.
    /// </summary>
    public interface ICurrencySelector
    {
        /// <summary>
        /// The current currency used for displaying prices to the customer.
        /// </summary>
        public ICurrency CurrentDisplayCurrency { get; }
    }
}
