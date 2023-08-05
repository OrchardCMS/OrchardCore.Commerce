using Lombiq.HelpfulLibraries.OrchardCore.Contents;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Settings;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Records;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;
using ISession=YesSql.ISession;

namespace OrchardCore.Commerce.Middlewares;

public class LocalizationCurrencyRedirectMiddleware
{
    private readonly RequestDelegate _next;

    public LocalizationCurrencyRedirectMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext httpContext)
    {
        var route = httpContext.GetRouteData().Values;
        return "Item".Equals(route.GetMaybe("controller")) &&
               "Display".Equals(route.GetMaybe("action")) &&
               route.TryGetValue("contentItemId", out var value) &&
               value is string id
            ? QueryAndRedirectAsync(httpContext, id)
            : _next(httpContext);
    }

    private async Task QueryAndRedirectAsync(HttpContext context, string id)
    {
        var contentManager = context.RequestServices.GetRequiredService<IContentManager>();
        var item = await contentManager.GetAsync(id);

        if (item?.As<PricePart>() is { } pricePart &&
            item.As<LocalizationPart>() is { } localizationPart &&
            await context.RequestServices.GetRequiredService<ISiteService>().GetSiteSettingsAsync() is { } settings &&
            settings.As<CurrencySettings>().CurrentDisplayCurrency is { } displayCurrency &&
            displayCurrency != pricePart.Price.Currency.CurrencyIsoCode)
        {
            var session = context.RequestServices.GetRequiredService<ISession>();
            var localizationSet = await session
                .QueryContentItem(PublicationStatus.Published)
                .Where(index => index.ContentItemId != item.ContentItemId)
                .With<LocalizedContentItemIndex>(index => index.LocalizationSet == localizationPart.LocalizationSet)
                .ListAsync();

            var applicable = localizationSet
                .As<PricePart>()
                .FirstOrDefault(part => part.Price.Currency.CurrencyIsoCode == displayCurrency);

            if (applicable != null)
            {
                var urlHelperFactory = context.RequestServices.GetRequiredService<IUrlHelperFactory>();
                var urlHelper = urlHelperFactory.GetUrlHelper(new ActionContext(
                    context,
                    context.GetRouteData(),
                    new ActionDescriptor()));
                context.Response.Redirect(urlHelper.DisplayContentItem(applicable));
            }
        }

        await _next(context);
    }
}
