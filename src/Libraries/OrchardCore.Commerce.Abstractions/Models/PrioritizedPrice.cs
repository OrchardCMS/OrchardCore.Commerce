using System.Diagnostics;
using System.Globalization;
using System.Text.Json.Serialization;
using OrchardCore.Commerce.Abstractions.Serialization;
using OrchardCore.Commerce.MoneyDataType;

namespace OrchardCore.Commerce.Abstractions.Models;

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

    private string DebuggerDisplay => string.Create(CultureInfo.InvariantCulture, $"{Price} ^{Priority}");

    public PrioritizedPrice(int priority, Amount price)
    {
        Priority = priority;
        Price = price;
    }
}
