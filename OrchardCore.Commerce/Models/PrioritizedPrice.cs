using System.Diagnostics;
using System.Text.Json.Serialization;
using Money;
using OrchardCore.Commerce.Serialization;

namespace OrchardCore.Commerce.Models
{
    /// <summary>
    /// A price and its priority.
    /// </summary>
    [JsonConverter(typeof(PrioritizedPriceConverter))]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class PrioritizedPrice
    {
        /// <summary>
        /// The priority for the price (higher takes precedence).
        /// </summary>
        public int Priority { get; }
        /// <summary>
        /// The price.
        /// </summary>
        public Amount Price { get; }

        /// <summary>
        /// Builds a new prioritized price from an amount and a priority.
        /// </summary>
        /// <param name="priority">The piority.</param>
        /// <param name="price">The price.</param>
        public PrioritizedPrice(int priority, Amount price)
        {
            Priority = priority;
            Price = price;
        }

        private string DebuggerDisplay => $"{Price} ^{Priority}";
    }
}
