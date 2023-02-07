using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.MoneyDataType;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Tax.Extensions;

public static class AdditionalDataExtensions
{
    private const string GrossPrice = nameof(GrossPrice);
    private const string NetPrice = nameof(NetPrice);

    private const string Old = nameof(Old);
    private const string OldGrossPrice = Old + GrossPrice;
    private const string OldNetPrice = Old + NetPrice;

    public static Amount GetGrossPrice(this IDictionary<string, JToken> additionalData) =>
        GetAmount(additionalData, GrossPrice) ?? Amount.Unspecified;

    public static void SetGrossPrice(this IDictionary<string, JToken> additionalData, Amount amount) =>
        additionalData[GrossPrice] = JToken.FromObject(amount);

    public static bool HasGrossPrice(this IDictionary<string, JToken> additionalData) =>
        additionalData.ContainsKey(GrossPrice);

    public static Amount GetNetPrice(this IDictionary<string, JToken> additionalData) =>
        GetAmount(additionalData, NetPrice) ?? Amount.Unspecified;

    public static void SetNetPrice(this IDictionary<string, JToken> additionalData, Amount amount) =>
        additionalData[NetPrice] = JToken.FromObject(amount);

    public static (Amount Net, Amount? Gross) GetOldPrices(this IDictionary<string, JToken> additionalData) =>
        (GetAmount(additionalData, OldNetPrice) ?? Amount.Unspecified, GetAmount(additionalData, OldGrossPrice));

    public static void SetOldPrices(this IDictionary<string, JToken> additionalData, Amount netAmount, Amount? grossAmount = null)
    {
        additionalData[OldNetPrice] = JToken.FromObject(netAmount);
        if (grossAmount is { } gross) additionalData[OldGrossPrice] = JToken.FromObject(gross);
    }

    private static Amount? GetAmount(IDictionary<string, JToken> additionalData, string key) =>
        additionalData.GetMaybe(key)?.ToObject<Amount>();
}
