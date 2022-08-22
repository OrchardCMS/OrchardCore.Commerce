using OrchardCore.Commerce.AddressDataType.Abstractions;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace OrchardCore.Commerce.AddressDataType;

/// <summary>
/// A flexible address formatter that can be built with a couple format strings.
/// </summary>
public class AddressFormatter :
    IAddressFormatter
{
    private readonly string _addressFormat;
    private readonly string _cityLineFormat;
    private readonly bool _uppercase;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddressFormatter"/> class.
    /// </summary>
    /// <param name="addressFormat">
    /// A format string for the address. Parameters are, in order:
    /// 0. The name.
    /// 1. The department.
    /// 2. The company or institution.
    /// 3. The first line of the street address.
    /// 4. The second line of the street address.
    /// 5. The city line (<see cref="_cityLineFormat"/>).
    /// 6. The country.
    /// </param>
    /// <param name="cityLineFormat">
    /// A format string for the city line of the address. Parameters are, in order:
    /// 0. The city.
    /// 1. The province or state.
    /// 2. The postal code.
    /// </param>
    /// <param name="uppercase">If <see langword="true"/>, the address is changed to uppercase after formatting.</param>
    public AddressFormatter(
        string addressFormat = @"{0}
{1}
{2}
{3}
{4}
{5}
{6}",
        string cityLineFormat = "{0} {1} {2}",
        bool uppercase = true)
    {
        _addressFormat = addressFormat;
        _cityLineFormat = cityLineFormat;
        _uppercase = uppercase;
    }

    /// <summary>
    /// Formats the address with the format strings provided.
    /// </summary>
    public string Format(Address address)
    {
        if (address is null) return "-";

        var rawFormatted = string.Format(
            CultureInfo.InvariantCulture,
            _addressFormat,
            address.Name,
            address.Department,
            address.Company,
            address.StreetAddress1,
            address.StreetAddress2,
            string.Format(CultureInfo.InvariantCulture, _cityLineFormat, address.City, address.Province, address.PostalCode),
            address.Region);
        var withoutEmptyLines = Regex
            .Replace(rawFormatted, @"(?<first>\r?\n)[\r\n]+", "${first}", RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(1))
            .Trim('\r', '\n');
        return _uppercase
            ? withoutEmptyLines.ToUpperInvariant()
            : withoutEmptyLines;
    }
}
