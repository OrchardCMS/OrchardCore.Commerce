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
    public class PriceBookByRolePartDisplayDriver : ContentPartDisplayDriver<PriceBookByRolePart>
    {
        private readonly IPriceBookService _priceBookService;
        private readonly IContentManager _contentManager;

        public PriceBookByRolePartDisplayDriver(
            IPriceBookService priceBookService,
            IContentManager contentManager)
        {
            _priceBookService = priceBookService;
            _contentManager = contentManager;
        }

        public override IDisplayResult Edit(PriceBookByRolePart priceBookByRolePart)
        {
            return Initialize<PriceBookByRolePartViewModel>("PriceBookByRolePart_Edit", async m => 
                await BuildViewModel(m, priceBookByRolePart)
            );
        }

        public override async Task<IDisplayResult> UpdateAsync(PriceBookByRolePart model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.RoleName);
            await updater.TryUpdateModelAsync(model, Prefix, t => t.PriceBookContentItemId);

            // Auto set the display text if no TitlePart
            if (!model.ContentItem.Has("TitlePart"))
            {
                model.ContentItem.DisplayText = string.Format("Role '{0}' assigned Price Book '{1}'",
                    model.RoleName,
                    (await _contentManager.GetAsync(model.PriceBookContentItemId)).DisplayText);
            }

            return Edit(model);
        }

        private async Task BuildViewModel(PriceBookByRolePartViewModel model, PriceBookByRolePart part)
        {
            model.RoleName = part.RoleName;
            model.PriceBookContentItemId = part.PriceBookContentItemId;

            model.PriceBooks = await _priceBookService.GetAllPriceBooks();

            return;
        }
    }
}
