using Money;

namespace OrchardCore.Commerce.Models
{
    public class OrderAdditionalCost
    {
        /// <summary>
        /// A string describing the kind of additional cost this covers, such as "shipping" or "taxes".
        /// </summary>
        public string Kind { get; set; }

        /// <summary>
        /// The description for that additional cost as it will appear in the order or invoice.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The amount that will br charged on top of the cost of items in the order.
        /// </summary>
        public Amount Cost { get; set; }
    }
}
