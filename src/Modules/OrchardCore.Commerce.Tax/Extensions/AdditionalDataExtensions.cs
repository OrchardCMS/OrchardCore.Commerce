using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.MoneyDataType;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Tax.Extensions;

public static class AdditionalDataExtensions
{
    private const string GrossPrice = nameof(GrossPrice);

    public static Amount GetGrossPrice(this IDictionary<string, JToken> additionalData) =>
        additionalData.GetMaybe(GrossPrice)?.ToObject<Amount>() ?? Amount.Unspecified;

    public static void SetGrossPrice(this IDictionary<string, JToken> additionalData, Amount amount) =>
        additionalData[GrossPrice] = JToken.FromObject(amount);
}
