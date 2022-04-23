using System.Diagnostics;
using System.Text.Json.Serialization;
using Money;
using OrchardCore.Commerce.Serialization;

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

    public PrioritizedPrice(int priority, Amount price)
    {
        Priority = priority;
        Price = price;
    }

    private string DebuggerDisplay => $"{Price} ^{Priority}";
}
