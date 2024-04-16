using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OrchardCore.Commerce.Payment.Exactly.Models;

public class ChargeAction : IExactlyResponseData
{
    public string Type => "action";

    public ChargeActionAttributes Attributes { get; set; }

    public class ChargeActionAttributes
    {
        public string Action { get; set; }
        public Uri Url { get; set; }
        public IEnumerable<object> Parameters { get; set; }
        public string HttpMethod { get; set; }

        [JsonIgnore]
        public bool IsGet => "GET".EqualsOrdinalIgnoreCase(HttpMethod);
    }
}
