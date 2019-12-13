using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Lists.Models;

namespace OrchardCore.Commerce.Drivers
{
    public class PriceBookEntryPartDisplayDriver : ContentPartDisplayDriver<PriceBookEntryPart>
    {
        private readonly IContentManager _contentManager;
        private readonly IPriceBookService _priceBookService;

        public PriceBookEntryPartDisplayDriver(
            IContentManager contentManager,
            IPriceBookService priceBookService)
        {
            _contentManager = contentManager;
            _priceBookService = priceBookService;
        }

        public override IDisplayResult Edit(PriceBookEntryPart priceBookEntryPart)
        {
            return Initialize<PriceBookEntryPartViewModel>("PriceBookEntryPart_Edit", async m => 
                await BuildViewModel(m, priceBookEntryPart)
            );
        }

        public override async Task<IDisplayResult> UpdateAsync(PriceBookEntryPart model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.ProductContentItemId);
            await updater.TryUpdateModelAsync(model, Prefix, t => t.UseStandardPrice);

            // Auto set the display text if no TitlePart
            if (!model.ContentItem.Has("TitlePart"))
            {
                if (model.UseStandardPrice)
                {
                    model.ContentItem.DisplayText = string.Format("Product '{0}' using 'Standard Price'",
                        (await _contentManager.GetAsync(model.ProductContentItemId)).DisplayText);
                }
                else
                {
                    model.ContentItem.DisplayText = string.Format("Product '{0}' - {1}",
                        (await _contentManager.GetAsync(model.ProductContentItemId)).DisplayText,
                        model.ContentItem.As<PricePart>().Price.ToString());
                }
            }

            return Edit(model);
        }

        private async Task BuildViewModel(PriceBookEntryPartViewModel model, PriceBookEntryPart part)
        {
            model.ProductContentItemId = part.ProductContentItemId;
            model.UseStandardPrice = part.UseStandardPrice;

            var standardPriceBook = await _priceBookService.GetStandardPriceBook();
            if (standardPriceBook != null)
            {
                var priceBookContentItemId = part.ContentItem.As<ContainedPart>()?.ListContentItemId;
                model.StandardPriceBook = standardPriceBook.ContentItem.ContentItemId == priceBookContentItemId;
            }

            if (!string.IsNullOrWhiteSpace(part.ProductContentItemId))
            {
                model.Product = await _contentManager.GetAsync(part.ProductContentItemId);
            }

            return;
        }
    }
}
