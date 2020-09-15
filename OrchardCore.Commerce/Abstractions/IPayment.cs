using Money;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IPayment
    {
        /// <summary>
        /// The kind of charge, such as "Credit Card", "Cash", "Bitcoin", atc.
        /// </summary>
        string Kind { get; }

        /// <summary>
        /// A unique ID for the transaction. The semantics of this can vary by provider.
        /// </summary>
        string TransactionId { get; }

        /// <summary>
        /// Text accompanying the charge. The semantics of this can vary by provider.
        /// </summary>
        string ChargeText { get; }

        /// <summary>
        /// Amount charged.
        /// </summary>
        Amount Amount { get; }
    }
}
