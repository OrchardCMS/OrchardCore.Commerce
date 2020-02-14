using System.Text.Json.Serialization;
using Money;
using OrchardCore.Commerce.Serialization;

namespace OrchardCore.Commerce.Models
{
    /// <summary>
    /// A price and its priority.
    /// </summary>
    [JsonConverter(typeof(PrioritizedPriceConverter))]
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

        public PrioritizedPrice(int priority, Amount price)
        {
            Priority = priority;
            Price = price;
        }
    }
}
