using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers
{
    public class PriceBookProductPartDisplayDriver : ContentPartDisplayDriver<PriceBookProductPart>
    {
        private readonly IPriceBookService _priceBookService;
        private readonly IContentManager _contentManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly YesSql.ISession _session;

        public PriceBookProductPartDisplayDriver(IPriceBookService priceBookService,
            IContentManager contentManager,
            IServiceProvider serviceProvider,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService,
            YesSql.ISession session)
        {
            _priceBookService = priceBookService;
            _contentManager = contentManager;
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
            _session = session;
        }

        public override IDisplayResult Display(PriceBookProductPart part)
        {
            return Combine(
                Initialize<PriceBookProductPartDisplayViewModel>("PriceBookProductPart", async m => 
                    await BuildDisplayViewModel(m, part)
                )
                .Location("Detail", "Content:25"),
                Initialize<PriceBookProductPartDisplayViewModel>("PriceBookProductPart_Summary", async m => 
                    await BuildDisplayViewModel(m, part)
                )
                .Location("Summary", "Meta:10")
            );
        }

        public override IDisplayResult Edit(PriceBookProductPart priceBookProductPart, BuildPartEditorContext context)
        {
            return Initialize<PriceBookProductPartEditViewModel>("PriceBookProductPart_Edit", async m =>
                    await BuildEditViewModel(m, priceBookProductPart, context.Updater)
                );
        }

        public override async Task<IDisplayResult> UpdateAsync(PriceBookProductPart part, UpdatePartEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var contentItemAuthorize = await _contentManager.NewAsync(CommerceConstants.ContentTypes.PriceBookEntry);
            if (!await _authorizationService.AuthorizeAsync(user, Contents.Permissions.PublishContent, contentItemAuthorize))
            {
                return Edit(part, context);
            }

            var contentItemDisplayManager = _serviceProvider.GetRequiredService<IContentItemDisplayManager>();
            
            var model = new PriceBookProductPartEditViewModel();
            await context.Updater.TryUpdateModelAsync(model, Prefix);

            // In the handler, these will be converted into permananet Price Book Entries
            var priceBookEntries = new List<PriceBookEntry>();
            for (var i = 0; i < model.Prefixes.Length; i++)
            {
                // Adding in a new Price Book Entry
                var priceBookEntry = await _contentManager.NewAsync("PriceBookEntry");
                await contentItemDisplayManager.UpdateEditorAsync(priceBookEntry, context.Updater, context.IsNew, htmlFieldPrefix: model.Prefixes[i]);
                var priceBookEntryPart = priceBookEntry.As<PriceBookEntryPart>();
                var pricePart = priceBookEntry.As<PricePart>();

                var priceBookProduct = new PriceBookEntry
                {
                    PriceBookEntryContentItemId = model.Prefixes[i],
                    PriceBookContentItemId = model.PriceBookContentItemIds[i],
                    UseStandardPrice = priceBookEntryPart.UseStandardPrice,
                    Price = pricePart.Price
                };

                priceBookEntries.Add(priceBookProduct);
            }

            part.TemporaryPriceBookEntries = priceBookEntries;

            return Edit(part, context);
        }

        private async Task BuildDisplayViewModel(PriceBookProductPartDisplayViewModel model, PriceBookProductPart part)
        {
            var priceBookPrices = Enumerable.Empty<PriceBookPrice>();

            var productPart = part.ContentItem.As<ProductPart>();
            if (productPart != null) {
                var priceBooks = await _priceBookService.GetPriceBooksFromRules();
                if (priceBooks.Any())
                {
                    priceBookPrices = await _priceBookService
                        .GetPriceBookPrices(priceBooks, productPart);
                }
            }

            model.PriceBookPrices = priceBookPrices;
            return;
        }

        private async Task BuildEditViewModel(PriceBookProductPartEditViewModel model, PriceBookProductPart part, IUpdateModel updater)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var contentItemAuthorize = await _contentManager.NewAsync(CommerceConstants.ContentTypes.PriceBookEntry);
            if (!await _authorizationService.AuthorizeAsync(user, Contents.Permissions.PublishContent, contentItemAuthorize))
            {
                return;
            }

            var productPart = part.ContentItem.As<ProductPart>();
            if (productPart != null) {
                model.PriceBookProductPart = part;
                model.PriceBooks = await _priceBookService.GetAllPriceBooks();
                model.PriceBookEntries = await _priceBookService.GetPriceBookEntriesByProduct(productPart.ContentItem.ContentItemId);
                model.Updater = updater;
            }
            return;
        }
    }
}
