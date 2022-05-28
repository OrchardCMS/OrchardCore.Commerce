using Money;
using OrchardCore.Commerce.Serialization;
using System;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace OrchardCore.Commerce.Models;

/// <summary>
/// A price and its priority.
/// </summary>
[JsonConverter(typeof(PrioritizedPriceConverter))]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class PrioritizedPrice
{
    /// <summary>
    /// Gets the priority for the price (higher takes precedence).
    /// </summary>
    public int Priority { get; }

    /// <summary>
    /// Gets the price.
    /// </summary>
    public Amount Price { get; }

    private string DebuggerDisplay => FormattableString.Invariant($"{Price} ^{Priority}");

    public PrioritizedPrice(int priority, Amount price)
    {
        Priority = priority;
        Price = price;
    }
}
