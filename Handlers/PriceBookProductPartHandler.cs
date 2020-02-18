using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Lists.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Handlers
{
    public class PriceBookProductPartHandler : ContentPartHandler<PriceBookProductPart>
    {
        // Using to prevent circular dependency injection
        private readonly IServiceProvider _serviceProvider;

        public PriceBookProductPartHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override async Task PublishedAsync(PublishContentContext context, PriceBookProductPart part)
        {
            var priceBookService = _serviceProvider.GetService<IPriceBookService>();
            var contentManager = _serviceProvider.GetService<IContentManager>();

            var productContentItemId = part.ContentItem.ContentItemId;
            var productTitle = part.ContentItem.DisplayText;

            var temporaryPriceBookEntries = part.TemporaryPriceBookEntries;
            var currentPriceBookEntries = await priceBookService.GetPriceBookEntriesByProduct(productContentItemId);

            foreach (var temporaryPriceBookEntry in temporaryPriceBookEntries)
            {
                var currentPriceBookEntry = currentPriceBookEntries
                    .Where(cpbe => cpbe.ContentItem.ContentItemId == temporaryPriceBookEntry.PriceBookEntryContentItemId)
                    .FirstOrDefault();

                ContentItem modifyingPriceBookEntry = currentPriceBookEntry == null ?
                    await contentManager.NewAsync("PriceBookEntry") :
                    currentPriceBookEntry.ContentItem;

                modifyingPriceBookEntry.Alter<ContainedPart>(p => p.ListContentItemId = temporaryPriceBookEntry.PriceBookContentItemId);
                modifyingPriceBookEntry.Alter<PriceBookEntryPart>(p => {
                    p.UseStandardPrice = temporaryPriceBookEntry.UseStandardPrice;
                    p.ProductContentItemId = productContentItemId;
                });
                modifyingPriceBookEntry.Alter<PricePart>(p => p.Price = temporaryPriceBookEntry.Price);
                var newPriceBookEntryPart = modifyingPriceBookEntry.As<PriceBookEntryPart>();
                modifyingPriceBookEntry.DisplayText = await priceBookService.GeneratePriceBookEntryTitle(newPriceBookEntryPart, productTitle);

                if (currentPriceBookEntry == null)
                {
                    await contentManager.CreateAsync(modifyingPriceBookEntry, VersionOptions.Published);
                }
                else
                {
                    await contentManager.UpdateAsync(modifyingPriceBookEntry);
                    await contentManager.PublishAsync(modifyingPriceBookEntry);
                }
            }

            await base.PublishedAsync(context, part);
        }
    }
}
