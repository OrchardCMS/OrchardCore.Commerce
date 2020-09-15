using System.Diagnostics;

namespace InternationalAddress
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Address
    {
        public string Name { get; set; }

        public string Department { get; set; }

        public string Company { get; set; }

        public string StreetAddress1 { get; set; }

        public string StreetAddress2 { get; set; }

        public string City { get; set; }

        public string Province { get; set; }

        public string PostalCode { get; set; }

        public string Region { get; set; }

        private string DebuggerDisplay => $"{Name}, {StreetAddress1}, {City}";
    }
}
