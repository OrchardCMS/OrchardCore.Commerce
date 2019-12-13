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
    /// A price book rule provider that obtains a price based on current user role
    /// </summary>
    public class PriceBookByRoleRuleProvider : IPriceBookRuleProvider
    {
        private readonly YesSql.ISession _session;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IContentManager _contentManager;

        private IEnumerable<PriceBookRule> priceBookRules;

        public PriceBookByRoleRuleProvider(YesSql.ISession session,
            IHttpContextAccessor httpContextAccessor,
            IContentManager contentManager)
        {
            _session = session;
            _httpContextAccessor = httpContextAccessor;
            _contentManager = contentManager;
        }

        public string Name { get { return "PriceBookByRole"; } }

        public async Task<IEnumerable<PriceBookRule>> GetPriceBookRules()
        {
            if (priceBookRules != null) return priceBookRules;

            var priceBookByUsers = await _session
                .ExecuteQuery(new ContentItemsByContentTypePublished(Name))
                .ListAsync();

            return priceBookRules = priceBookByUsers.Select(
                    p => new PriceBookByRoleRule(p.As<PriceBookByRolePart>(), _httpContextAccessor)
                );
        }
    }
}
