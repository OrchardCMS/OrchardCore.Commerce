using OrchardCore.Commerce.MoneyDataType;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Represents an object that contains some of an order line item's basic info.
/// </summary>
public interface ILineItem
{
    /// <summary>
    /// Gets or sets how many of this item is ordered.
    /// </summary>
    int Quantity { get; set; }

    /// <summary>
    /// Gets or sets a single item's price.
    /// </summary>
    Amount UnitPrice { get; set; }

    /// <summary>
    /// Gets or sets the subtotal of this line item.
    /// </summary>
    Amount LinePrice { get; set; }
}
