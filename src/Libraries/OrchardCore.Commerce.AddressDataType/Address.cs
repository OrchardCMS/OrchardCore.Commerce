using OrchardCore.Commerce.AddressDataType.Constants;
using System.Collections.Generic;
using System.Diagnostics;

namespace OrchardCore.Commerce.AddressDataType;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class Address
{
    private string DebuggerDisplay => $"{Name}, {StreetAddress1}, {City}";

    public string Name { get; set; }

    public string Department { get; set; }

    public string Company { get; set; }

    public string StreetAddress1 { get; set; }

    public string StreetAddress2 { get; set; }

    public string City { get; set; }

    public string Province { get; set; }

    public string PostalCode { get; set; }

    public string Region { get; set; }

    /// <summary>
    /// Gets a collection of name metadata. Some typical keys can be found in <see cref="CommonNameParts"/>.
    /// </summary>
    public IDictionary<string, string> NameParts { get; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets a collection of other address metadata not related to names.
    /// </summary>
    public IDictionary<string, string> AdditionalFields { get; } = new Dictionary<string, string>();
}
