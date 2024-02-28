using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.Promotion.Models;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Promotion.Extensions;

public static class AdditionalDataExtensions
{
    private const string Discounts = nameof(Discounts);

    public static IEnumerable<DiscountInformation> GetDiscounts(this IDictionary<string, JToken> additionalData) =>
        additionalData
            .GetMaybe(Discounts)?
            .ToObject<IEnumerable<DiscountInformation>>() ?? Enumerable.Empty<DiscountInformation>();

    public static void SetDiscounts(
        this IDictionary<string, JToken> additionalData,
        IEnumerable<DiscountInformation> discounts) =>
        additionalData[Discounts] = JToken.FromObject(discounts ?? Enumerable.Empty<DiscountInformation>());

    public static IDictionary<string, IEnumerable<DiscountInformation>> GetDiscountsByProduct(
        this IDictionary<string, JToken> additionalData) =>
        additionalData
            .GetMaybe(Discounts)?
            .ToObject<Dictionary<string, IEnumerable<DiscountInformation>>>()
        ?? [];

    public static void SetDiscountsByProduct(
        this IDictionary<string, JToken> additionalData,
        IDictionary<string, IEnumerable<DiscountInformation>> discounts) =>
        additionalData[Discounts] = JToken.FromObject(
            discounts ?? new Dictionary<string, IEnumerable<DiscountInformation>>());
}
