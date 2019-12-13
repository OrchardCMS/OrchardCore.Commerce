using System.Collections.Generic;
using OrchardCore.Commerce.Abstractions;

namespace OrchardCore.Commerce.ViewModels
{
    public class PriceBookRulesIndexViewModel
    {
        public IList<PriceBookRuleEntry> PriceBookRules { get; set; }
        public PriceBookRuleIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
        public IEnumerable<string> PriceBookRuleSourceNames { get; set; }
    }

    public class PriceBookRuleEntry
    {
        public PriceBookRule PriceBookRule { get; set; }
        public dynamic Shape { get; set; }
    }

    public class PriceBookRuleIndexOptions
    {
        public string Search { get; set; }
    }
}
