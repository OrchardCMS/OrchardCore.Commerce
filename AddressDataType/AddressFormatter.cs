using System;

namespace InternationalAddress
{
    /// <summary>
    /// A flexible address formatter that can be built with a couple format strings.
    /// </summary>
    public class AddressFormatter : IAddressFormatter
    {
        private readonly string _addressFormat;
        private readonly string _cityLineFormat;
        private readonly bool _uppercase;

        /// <summary>
        /// Constructs a specialized address formatter from a couple of format strings.
        /// </summary>
        /// <param name="addressFormat">
        /// A format string for the address. Parameters are, in order:
        /// 0. The name
        /// 1. The department
        /// 2. The company or institution
        /// 3. The first line of the street address
        /// 4. The second line of the street address
        /// 5. The city line (<see cref="cityLineFormat"/>)
        /// 6. The country
        /// </param>
        /// <param name="cityLineFormat">
        /// A format string for the city line of the address. Parameters are, in order:
        /// 0. The city
        /// 1. The province or state
        /// 2. The postal code
        /// </param>
        /// <param name="uppercase">If true, the address is changed to uppercase after formatting.</param>
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
        /// Formats the address with the format strings provided
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public string Format(Address address)
        {
            if (address is null) return "-";
            string rawFormatted = String.Format(
                _addressFormat,
                address.Name,
                address.Department,
                address.Company,
                address.StreetAddress1,
                address.StreetAddress2,
                String.Format(_cityLineFormat, address.City, address.Province, address.PostalCode),
                address.Region);
            string withoutEmptyLines = String.Join("", rawFormatted.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
            return _uppercase ? withoutEmptyLines.ToUpper() : withoutEmptyLines;
        }
    }
}
