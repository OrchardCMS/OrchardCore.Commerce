using OrchardCore.Commerce.Money;

namespace OrchardCore.Commerce.Abstractions
{
    /// <summary>
    /// A price object
    /// </summary>
    public interface IPrice
    {
        /// <summary>
        /// The amount object for this price
        /// </summary>
        Amount Price { get; }
    }
}
