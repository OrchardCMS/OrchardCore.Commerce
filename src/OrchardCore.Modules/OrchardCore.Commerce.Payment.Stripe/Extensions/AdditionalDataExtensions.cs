using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace OrchardCore.Commerce.Payment.Stripe.Extensions;

public static class AdditionalDataExtensions
{
    private const string PriceIds = nameof(PriceIds);

    public static IEnumerable<string> GetPriceIds(this IDictionary<string, JsonNode> additionalData) =>
        additionalData
            .GetMaybe(PriceIds)?
            .ToObject<IEnumerable<string>>() ?? [];

    public static void SetPriceIds(
        this IDictionary<string, JsonNode> additionalData,
        IEnumerable<string> priceIds) =>
        additionalData[PriceIds] = JArray.FromObject(priceIds ?? []);

    public static IDictionary<string, IEnumerable<string>> GetPriceIdsByProduct(
        this IDictionary<string, JsonNode> additionalData) =>
        additionalData
            .GetMaybe(PriceIds)?
            .ToObject<Dictionary<string, IEnumerable<string>>>()
        ?? [];

    public static void SetPriceIdsByProduct(
        this IDictionary<string, JsonNode> additionalData,
        IDictionary<string, IEnumerable<string>> priceIds) =>
        additionalData[PriceIds] = JObject.FromObject(
            priceIds ?? new Dictionary<string, IEnumerable<string>>());
}
