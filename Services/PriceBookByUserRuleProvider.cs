using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.CompiledQueries;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Services
{
    /// <summary>
    /// A price book rule provider that obtains a price based on current user
    /// </summary>
    public class PriceBookByUserRuleProvider : IPriceBookRuleProvider
    {
        private readonly YesSql.ISession _session;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IContentManager _contentManager;

        private IEnumerable<PriceBookRule> priceBookRules;

        public PriceBookByUserRuleProvider(YesSql.ISession session,
            IHttpContextAccessor httpContextAccessor,
            IContentManager contentManager)
        {
            _session = session;
            _httpContextAccessor = httpContextAccessor;
            _contentManager = contentManager;
        }

        public string Name { get { return "PriceBookByUser"; } }

        public async Task<IEnumerable<PriceBookRule>> GetPriceBookRules()
        {
            if (priceBookRules != null) return priceBookRules;

            var priceBookByUsers = await _session
                .ExecuteQuery(new ContentItemsByContentTypePublished(Name))
                .ListAsync();

            return priceBookRules = priceBookByUsers.Select(
                    p => new PriceBookByUserRule(p.As<PriceBookByUserPart>(), _httpContextAccessor)
                );
        }
    }
}
