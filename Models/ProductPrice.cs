using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Money;

namespace OrchardCore.Commerce.Models
{
    /// <summary>
    /// A simple product price implementation
    /// </summary>
    public class ProductPrice : IPrice
    {
        public Amount Price { get; }

        public ProductPrice(Amount price)
        {
            Price = price;
        }
    }
}
