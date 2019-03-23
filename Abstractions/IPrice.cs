using OrchardCore.Commerce.Money;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IPrice
    {
        Amount Price { get; }
    }
}
