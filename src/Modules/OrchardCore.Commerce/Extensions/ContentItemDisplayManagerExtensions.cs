using OrchardCore.DisplayManagement;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Constants.ContentTypes;

namespace OrchardCore.ContentManagement.Display;

public static class ContentItemDisplayManagerExtensions
{
    public static Task<IShape> BuildMvcTitleAsync(this IContentItemDisplayManager manager, string text)
    {
        var header = new ContentItem { ContentType = MvcTitle, DisplayText = text };
        return manager.BuildDisplayAsync(header, updater: null);
    }
}
