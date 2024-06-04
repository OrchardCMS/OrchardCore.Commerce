using System.Collections.Generic;
using System.Text.Json.Nodes;
using OrchardCore.Commerce.Promotion.Models;

namespace OrchardCore.Commerce.Promotion.Extensions;

public static class AdditionalDataExtensions
{
    private const string Discounts = nameof(Discounts);

    public static IEnumerable<DiscountInformation> GetDiscounts(this IDictionary<string, JsonNode> additionalData) =>
        additionalData
            .GetMaybe(Discounts)?
            .ToObject<IEnumerable<DiscountInformation>>() ?? [];

    public static void SetDiscounts(
        this IDictionary<string, JsonNode> additionalData,
        IEnumerable<DiscountInformation> discounts) =>
        additionalData[Discounts] = JArray.FromObject(discounts ?? []);

    public static IDictionary<string, IEnumerable<DiscountInformation>> GetDiscountsByProduct(
        this IDictionary<string, JsonNode> additionalData) =>
        additionalData
            .GetMaybe(Discounts)?
            .ToObject<Dictionary<string, IEnumerable<DiscountInformation>>>()
        ?? [];

    public static void SetDiscountsByProduct(
        this IDictionary<string, JsonNode> additionalData,
        IDictionary<string, IEnumerable<DiscountInformation>> discounts) =>
        additionalData[Discounts] = JObject.FromObject(
            discounts ?? new Dictionary<string, IEnumerable<DiscountInformation>>());
}
