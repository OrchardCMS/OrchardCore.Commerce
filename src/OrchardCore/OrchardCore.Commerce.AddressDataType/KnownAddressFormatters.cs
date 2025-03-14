using OrchardCore.Commerce.AddressDataType.Abstractions;
using System;
using System.Collections.Generic;

namespace OrchardCore.Commerce.AddressDataType;

internal static class KnownAddressFormatters
{
    public static readonly IAddressFormatter DefaultFormatter = new DefaultAddressFormatter();

    public static readonly IDictionary<string, IAddressFormatter> Formatters
        = new Dictionary<string, IAddressFormatter>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "US", DefaultFormatter },
            { "FR", new AddressFormatter(cityLineFormat: "{2} {0}") },
        };
}
