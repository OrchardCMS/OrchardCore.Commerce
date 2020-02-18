using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Abstractions
{
    /// <summary>
    /// A price book rule object
    /// </summary>
    public abstract class PriceBookRule: IContent
    {
        abstract public string Name { get; }
        abstract public decimal Weight { get; }
        abstract public string PriceBookContentItemId { get; }
        abstract public ContentItem ContentItem { get; }

        abstract public bool Applies();
    }
}
