using System;
using System.Collections.Generic;

namespace InternationalAddress
{
    internal static class KnownAddressFormatters
    {
        public static readonly IAddressFormatter DefaultFormatter = new DefaultAddressFormatter();

        // TODO: complete this table
        public static readonly IDictionary<string, IAddressFormatter> Formatters
            = new Dictionary<string, IAddressFormatter>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "US", DefaultFormatter },
            { "FR", new AddressFormatter(cityLineFormat: "{2} {0}") }
        };
    }
}
