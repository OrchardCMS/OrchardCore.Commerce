using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Commerce.Drivers
{
    public class PriceBookPartDisplayDriver : ContentPartDisplayDriver<PriceBookPart>
    {
        private readonly IPriceBookService _priceBookService;

        public PriceBookPartDisplayDriver(IPriceBookService priceBookService)
        {
            _priceBookService = priceBookService;
        }

        public override IDisplayResult Edit(PriceBookPart priceBookPart)
        {
            return Initialize<PriceBookPartViewModel>("PriceBookPart_Edit", async m => 
                await BuildViewModel(m, priceBookPart)
            );
        }

        public override async Task<IDisplayResult> UpdateAsync(PriceBookPart model, IUpdateModel updater)
        {
            // We only allow checking "Standard Price Book", not unchecking
            var viewModel = new PriceBookPartViewModel();
            await updater.TryUpdateModelAsync(viewModel, Prefix, t => t.StandardPriceBook);

            if (viewModel.StandardPriceBook)
            {
                await _priceBookService.SetStandardPriceBook(model);
            }

            return Edit(model);
        }

        private async Task BuildViewModel(PriceBookPartViewModel model, PriceBookPart part)
        {
            var standardPriceBook = part.StandardPriceBook;
            if (!standardPriceBook)
            {
                // If no standard price book is set, default the viewmodel to having this be it
                standardPriceBook = (await _priceBookService.GetStandardPriceBook()) == null;
            }

            model.StandardPriceBook = standardPriceBook;

            return;
        }
    }
}
