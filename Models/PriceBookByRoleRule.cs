using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Models
{
    public class PriceBookByRoleRule : PriceBookRule
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PriceBookByRolePart _priceBookByRolePart;

        public PriceBookByRoleRule(PriceBookByRolePart priceBookByRolePart,
            IHttpContextAccessor httpContextAccessor)
        {
            _priceBookByRolePart = priceBookByRolePart;
            _httpContextAccessor = httpContextAccessor;
        }

        // PriceBookRule Implementation
        public override string Name => ContentItem?.ContentItem.DisplayText;

        public override decimal Weight => ContentItem?.ContentItem.As<PriceBookRulePart>().Weight ?? 0;

        public override string PriceBookContentItemId => ContentItem?.ContentItem.As<PriceBookByRolePart>().PriceBookContentItemId;

        public override IContent ContentItem => _priceBookByRolePart?.ContentItem;

        public override bool Applies()
        {
            if (_priceBookByRolePart == null) return false;

            return _httpContextAccessor.HttpContext.User.IsInRole(_priceBookByRolePart.RoleName);
        }
    }
}
