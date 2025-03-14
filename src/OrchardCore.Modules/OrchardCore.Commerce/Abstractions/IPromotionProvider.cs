using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Promotion providers apply promotions to shopping cart items.
/// </summary>
public interface IPromotionProvider : ISortableUpdaterProvider<PromotionAndTaxProviderContext>
{ }
