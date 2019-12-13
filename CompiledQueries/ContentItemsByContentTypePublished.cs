using System;
using System.Linq.Expressions;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;

namespace OrchardCore.Commerce.CompiledQueries
{
    public class ContentItemsByContentTypePublished : ICompiledQuery<ContentItem>
    {
        public string ContentType { get; set; }

        public ContentItemsByContentTypePublished(string contentType)
        {
            ContentType = contentType;
        }

        public Expression<Func<IQuery<ContentItem>, IQuery<ContentItem>>> Query()
        {
            return query => query
                .With<ContentItemIndex>(x => x.ContentType == ContentType
                    && x.Published == true);
        }
    }
}
