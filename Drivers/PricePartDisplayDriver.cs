using System.Threading.Tasks;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Commerce.Drivers
{
    public class PricePartDisplayDriver : ContentPartDisplayDriver<PricePart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public PricePartDisplayDriver(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Display(PricePart pricePart)
        {
            return Combine(
                Initialize<PricePartViewModel>("PricePart", m => BuildViewModel(m, pricePart))
                    .Location("Detail", "Content:25"),
                Initialize<PricePartViewModel>("PricePart_Summary", m => BuildViewModel(m, pricePart))
                    .Location("Summary", "Meta:10")
            );
        }

        public override IDisplayResult Edit(PricePart pricePart)
        {
            return Initialize<PricePartViewModel>("PricePart_Edit", m => BuildViewModel(m, pricePart));
        }

        public override async Task<IDisplayResult> UpdateAsync(PricePart model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.Price);

            return Edit(model);
        }

        private Task BuildViewModel(PricePartViewModel model, PricePart part)
        {
            model.ContentItem = part.ContentItem;
            model.Price = part.Price;
            model.PricePart = part;

            return Task.CompletedTask;
        }
    }
}
