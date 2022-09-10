using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Updates prices with tax.
/// </summary>
public interface ITaxProvider : ISortableUpdaterProvider<TaxProviderContext>
{ }
