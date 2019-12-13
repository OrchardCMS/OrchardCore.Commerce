using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Abstractions
{
    /// <summary>
    /// A price book rule object
    /// </summary>
    public abstract class PriceBookRule
    {
        abstract public string Name { get; }
        abstract public decimal Weight { get; }
        abstract public string PriceBookContentItemId { get; }
        abstract public IContent ContentItem { get; }

        abstract public bool Applies();
    }
}
