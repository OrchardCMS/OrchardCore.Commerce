using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Models
{
    public class PriceBookByUserRule : PriceBookRule
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PriceBookByUserPart _priceBookByUserPart;

        public PriceBookByUserRule(PriceBookByUserPart priceBookByUserPart,
            IHttpContextAccessor httpContextAccessor)
        {
            _priceBookByUserPart = priceBookByUserPart;
            _httpContextAccessor = httpContextAccessor;
        }

        // PriceBookRule Implementation
        public override string Name => ContentItem?.ContentItem.DisplayText;

        public override decimal Weight => ContentItem?.ContentItem.As<PriceBookRulePart>().Weight ?? 0;

        public override string PriceBookContentItemId => ContentItem?.ContentItem.As<PriceBookByUserPart>().PriceBookContentItemId;

        public override IContent ContentItem => _priceBookByUserPart?.ContentItem;

        public override bool Applies()
        {
            if (_priceBookByUserPart == null) return false;

            var userName = _httpContextAccessor.HttpContext.User.Identity.Name;
            return _priceBookByUserPart.UserName == userName;
        }
    }
}
