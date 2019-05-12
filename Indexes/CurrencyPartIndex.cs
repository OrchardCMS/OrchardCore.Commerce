using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Money;
using OrchardCore.ContentManagement;
using YesSql.Indexes;

namespace OrchardCore.Commerce.Indexes
{
    /// <summary>
    /// 
    /// </summary>
    public class CurrencyPartIndex : MapIndex
    {
        public string ContentItemId { get; set; }
        public string IsoCode { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class CurrencyPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<CurrencyPartIndex>()
                .Map(contentItem =>
                {
                    if (!contentItem.IsPublished())
                    {
                        return null;
                    }

                    var currencyPart = contentItem.As<CurrencyPart>();

                    if (currencyPart?.IsoCode == null)
                    {
                        return null;
                    }

                    return new CurrencyPartIndex
                    {
                        IsoCode = currencyPart.IsoCode.ToUpperInvariant(),
                        ContentItemId = contentItem.ContentItemId,
                    };
                });
        }
    }
}
