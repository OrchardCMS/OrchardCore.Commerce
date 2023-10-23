using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Represents <see cref="IContent"/> such as content item or content part that has an <see cref="ISkuHolder.Sku"/>
/// property.
/// </summary>
public interface ISkuHolderContent : ISkuHolder, IContent
{
}
