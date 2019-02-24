using OrchardCore.Commerce.Abstractions;

namespace OrchardCore.Commerce.Models
{
    public class ProductPrice : IPrice
    {
        public decimal Price { get; }

        public ProductPrice(decimal price)
        {
            Price = price;
        }
    }
}
