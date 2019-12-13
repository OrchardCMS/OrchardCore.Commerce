using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Admin;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.Commerce.Controllers
{
    [Admin]
    public class PriceBookRulesAdminController : Controller, IUpdateModel
    {
        private readonly IEnumerable<IPriceBookRuleProvider> _priceBookRuleProviders;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISiteService _siteService;
        private readonly IDisplayManager<PriceBookRule> _displayManager;

        public PriceBookRulesAdminController(
            IEnumerable<IPriceBookRuleProvider> priceBookRuleProviders,
            IAuthorizationService authorizationService,
            ISiteService siteService,
            IDisplayManager<PriceBookRule> displayManager,
            IShapeFactory shapeFactory)
        {
            _priceBookRuleProviders = priceBookRuleProviders;
            _authorizationService = authorizationService;
            _siteService = siteService;
            _displayManager = displayManager;

            New = shapeFactory;
        }

        public dynamic New { get; set; }

        [HttpGet]
        public async Task<ActionResult> Index(PriceBookRuleIndexOptions options, PagerParameters pagerParameters)
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
                options = new PriceBookRuleIndexOptions();
            }

            var priceBookRules = new List<PriceBookRule>();
            foreach (var priceBookRuleProvider in _priceBookRuleProviders)
            {
                priceBookRules.AddRange(
                    await priceBookRuleProvider.GetPriceBookRules()
                );
            }

            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                priceBookRules = priceBookRules
                    .Where(p => p.Name.IndexOf(options.Search, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }

            var results = priceBookRules
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize)
                .ToList();

            // Maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Search", options.Search);

            var pagerShape = (await New.Pager(pager)).TotalItemCount(priceBookRules.Count).RouteData(routeData);

            var model = new PriceBookRulesIndexViewModel()
            {
                PriceBookRules = new List<PriceBookRuleEntry>(),
                Options = options,
                Pager = pagerShape,
                PriceBookRuleSourceNames = _priceBookRuleProviders.Select(x => x.Name)                
            };

            foreach (var priceBookRule in results)
            {
                model.PriceBookRules.Add(new PriceBookRuleEntry
                {
                    PriceBookRule = priceBookRule,
                    Shape = await _displayManager.BuildDisplayAsync(priceBookRule, this, "SummaryAdmin") 
                });
            }

            return View(model);
        }
    }
}
