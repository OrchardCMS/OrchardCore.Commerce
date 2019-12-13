using System.Threading.Tasks;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Commerce.Drivers
{
    public class PriceBookRulePartDisplayDriver : ContentPartDisplayDriver<PriceBookRulePart>
    {
        public override IDisplayResult Edit(PriceBookRulePart priceBookRulePart)
        {
            return Initialize<PriceBookRulePartViewModel>("PriceBookRulePart_Edit", m => BuildViewModel(m, priceBookRulePart));
        }

        public override async Task<IDisplayResult> UpdateAsync(PriceBookRulePart model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.Weight);
            
            return Edit(model);
        }

        private Task BuildViewModel(PriceBookRulePartViewModel model, PriceBookRulePart part)
        {
            model.Weight = part.Weight;

            return Task.CompletedTask;
        }
    }
}
