using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Admin;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Navigation;
using OrchardCore.Settings;
using YesSql;

namespace OrchardCore.Commerce.Controllers
{
    [Admin]
    public class PriceBooksAdminController : Controller, IUpdateModel
    {
        private readonly IPriceBookService _priceBookService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISiteService _siteService;
        private readonly IContentItemDisplayManager _displayManager;
        private readonly ISession _session;

        public PriceBooksAdminController(
            IPriceBookService priceBookService,
            IAuthorizationService authorizationService,
            ISiteService siteService,
            IContentItemDisplayManager displayManager,
            ISession session,
            IShapeFactory shapeFactory)
        {
            _priceBookService = priceBookService;
            _authorizationService = authorizationService;
            _siteService = siteService;
            _displayManager = displayManager;
            _session = session;

            New = shapeFactory;
        }

        public dynamic New { get; set; }

        [HttpGet]
        public async Task<ActionResult> Index(PriceBookIndexOptions options, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageCommerceSettings))
            {
                return Unauthorized();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);

            // default options
            if (options == null)
            {
                options = new PriceBookIndexOptions();
            }

            var query = _session.Query<ContentItem, ContentItemIndex>();
            query = query.With<ContentItemIndex>(x => x.Latest);
            query = query.With<ContentItemIndex>(x => x.ContentType == CommerceConstants.ContentTypes.PriceBook);
            
            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                query = query.With<ContentItemIndex>(x => x.DisplayText.Contains(options.Search));
            }
            
            // Maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Search", options.Search);

            var pagerShape = (await New.Pager(pager)).TotalItemCount(await query.CountAsync()).RouteData(routeData);
            var pageOfContentItems = await query.Skip(pager.GetStartIndex()).Take(pager.PageSize).ListAsync();

            // We preapre the content items SummaryAdmin shape
            var contentItemSummaries = new List<dynamic>();
            foreach (var contentItem in pageOfContentItems)
            {
                contentItemSummaries.Add(await _displayManager.BuildDisplayAsync(contentItem, this, "SummaryAdmin"));
            }

            var model = new PriceBooksIndexViewModel()
            {
                ContentItems = contentItemSummaries,
                Options = options,
                Pager = pagerShape
            };
            
            return View(model);
        }
    }
}
