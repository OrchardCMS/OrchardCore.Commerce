using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Money;

namespace OrchardCore.Commerce.Models
{
    public class ProductPrice : IPrice
    {
        public Amount Price { get; }

        public ProductPrice(Amount price)
        {
            Price = price;
        }
    }
}
