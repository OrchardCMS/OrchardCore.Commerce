using System;
using System.Linq.Expressions;
using OrchardCore.Commerce.Indexes;
using YesSql;

namespace OrchardCore.Commerce.CompiledQueries
{
    public class PriceBookEntryByPriceBook : ICompiledQuery<PriceBookEntryPartIndex>
    {
        public string PriceBookContentItemId { get; set; }

        public PriceBookEntryByPriceBook(string priceBookContentItemId)
        {
            PriceBookContentItemId = priceBookContentItemId;
        }

        public Expression<Func<IQuery<PriceBookEntryPartIndex>, IQuery<PriceBookEntryPartIndex>>> Query()
        {
            return query => query
                .With<PriceBookEntryPartIndex>(x => x.PriceBookContentItemId == PriceBookContentItemId);
        }
    }
}
