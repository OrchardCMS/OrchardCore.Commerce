using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.MoneyDataType;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Tax.Extensions;

public static class AdditionalDataExtensions
{
    private const string GrossPrice = nameof(GrossPrice);
    private const string NetPrice = nameof(NetPrice);
    private const string OldPrice = nameof(OldPrice);

    public static Amount GetGrossPrice(this IDictionary<string, JToken> additionalData) =>
        additionalData.GetMaybe(GrossPrice)?.ToObject<Amount>() ?? Amount.Unspecified;

    public static void SetGrossPrice(this IDictionary<string, JToken> additionalData, Amount amount) =>
        additionalData[GrossPrice] = JToken.FromObject(amount);

    public static bool HasGrossPrice(this IDictionary<string, JToken> additionalData) =>
        additionalData.ContainsKey(GrossPrice);

    public static Amount GetNetPrice(this IDictionary<string, JToken> additionalData) =>
        additionalData.GetMaybe(NetPrice)?.ToObject<Amount>() ?? Amount.Unspecified;

    public static void SetNetPrice(this IDictionary<string, JToken> additionalData, Amount amount) =>
        additionalData[NetPrice] = JToken.FromObject(amount);

    public static Amount GetOldPrice(this IDictionary<string, JToken> additionalData) =>
        additionalData.GetMaybe(OldPrice)?.ToObject<Amount>() ?? Amount.Unspecified;

    public static void SetOldPrice(this IDictionary<string, JToken> additionalData, Amount amount) =>
        additionalData[OldPrice] = JToken.FromObject(amount);
}
