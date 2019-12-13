using System;
using System.Linq.Expressions;
using OrchardCore.Commerce.Indexes;
using YesSql;

namespace OrchardCore.Commerce.CompiledQueries
{
    public class PriceBookEntryByProduct : ICompiledQuery<PriceBookEntryPartIndex>
    {
        public string ProductContentItemId { get; set; }

        public PriceBookEntryByProduct(string productContentItemId)
        {
            ProductContentItemId = productContentItemId;
        }

        public Expression<Func<IQuery<PriceBookEntryPartIndex>, IQuery<PriceBookEntryPartIndex>>> Query()
        {
            return query => query
                .With<PriceBookEntryPartIndex>(x => x.ProductContentItemId == ProductContentItemId);
        }
    }
}
