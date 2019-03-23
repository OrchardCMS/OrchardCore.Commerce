using System;

namespace OrchardCore.Commerce.Abstractions
{
    public interface ICurrency : IEquatable<ICurrency>
    {
        string Symbol { get; }
        string Name { get; }
        string IsoCode { get; }
        int DecimalPlaces { get; }
        bool IsResolved { get; }

        string ToString(decimal amount);
    }
}
