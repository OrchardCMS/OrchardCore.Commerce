using Money;

namespace OrchardCore.Commerce.Models;

public class OrderAdditionalCost
{
    /// <summary>
    /// Gets or sets a string describing the kind of additional cost this covers, such as "shipping" or "taxes".
    /// </summary>
    public string Kind { get; set; }

    /// <summary>
    /// Gets or sets the description for that additional cost as it will appear in the order or invoice.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the amount that will be charged on top of the cost of the items in the order.
    /// </summary>
    public Amount Cost { get; set; }
}
