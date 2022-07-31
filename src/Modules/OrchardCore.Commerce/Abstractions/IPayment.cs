using Money;
using System;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Describes a payment transaction's details.
/// </summary>
public interface IPayment
{
    /// <summary>
    /// Gets the kind of charge, such as "Credit Card", "Cash", "Bitcoin", atc.
    /// </summary>
    string Kind { get; }

    /// <summary>
    /// Gets a unique ID for the transaction. The semantics of this can vary by provider.
    /// </summary>
    string TransactionId { get; }

    /// <summary>
    /// Gets the text accompanying the charge. The semantics of this can vary by provider.
    /// </summary>
    string ChargeText { get; }

    /// <summary>
    /// Gets the amount charged.
    /// </summary>
    Amount Amount { get; }

    /// <summary>
    /// Gets the UTC creation date and time of the charge.
    /// </summary>
    DateTime CreatedUtc { get; }
}
