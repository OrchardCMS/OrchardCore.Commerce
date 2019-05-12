using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Commerce.Drivers
{
    public class CurrencyPartDisplayDriver : ContentPartDisplayDriver<CurrencyPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public CurrencyPartDisplayDriver(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Edit(CurrencyPart part)
        {
            return Initialize<CurrencyPartViewModel>("CurrencyPart_Edit", model => BuildViewModel(model, part));
        }

        public override async Task<IDisplayResult> UpdateAsync(CurrencyPart part, IUpdateModel updater)
        {
            var model = new CurrencyPartViewModel();

            if (await updater.TryUpdateModelAsync(model, Prefix))
            {
                part.Name = model.Name;
                part.IsoCode = model.IsoCode;
                part.Symbol = model.Symbol;
                part.Culture = model.Culture;
                part.DecimalPlaces = model.DecimalPlaces;
            }

            return Edit(part);
        }

        private Task BuildViewModel(CurrencyPartViewModel model, CurrencyPart part)
        {
            model.Name = part.Name;
            model.IsoCode = part.IsoCode;
            model.Symbol = part.Symbol;
            model.Culture = part.Culture;
            model.DecimalPlaces = part.DecimalPlaces;

            return Task.CompletedTask;
        }
    }
}
