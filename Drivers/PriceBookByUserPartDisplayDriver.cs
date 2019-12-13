using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Commerce.Drivers
{
    public class PriceBookByUserPartDisplayDriver : ContentPartDisplayDriver<PriceBookByUserPart>
    {
        private readonly IPriceBookService _priceBookService;
        private readonly IContentManager _contentManager;

        public PriceBookByUserPartDisplayDriver(
            IPriceBookService priceBookService,
            IContentManager contentManager)
        {
            _priceBookService = priceBookService;
            _contentManager = contentManager;
        }

        public override IDisplayResult Edit(PriceBookByUserPart priceBookByUserPart)
        {
            return Initialize<PriceBookByUserPartViewModel>("PriceBookByUserPart_Edit", async m => 
                await BuildViewModel(m, priceBookByUserPart)
            );
        }

        public override async Task<IDisplayResult> UpdateAsync(PriceBookByUserPart model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.UserName);
            await updater.TryUpdateModelAsync(model, Prefix, t => t.PriceBookContentItemId);

            // Auto set the display text if no TitlePart
            if (!model.ContentItem.Has("TitlePart"))
            {
                model.ContentItem.DisplayText = string.Format("User '{0}' assigned Price Book '{1}'",
                    model.UserName,
                    (await _contentManager.GetAsync(model.PriceBookContentItemId)).DisplayText);
            }

            return Edit(model);
        }

        private async Task BuildViewModel(PriceBookByUserPartViewModel model, PriceBookByUserPart part)
        {
            model.UserName = part.UserName;
            model.PriceBookContentItemId = part.PriceBookContentItemId;

            model.PriceBooks = await _priceBookService.GetAllPriceBooks();

            return;
        }
    }
}
