using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Settings;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Commerce.Handlers
{
    public class PricePartHandler : ContentPartHandler<PricePart>
    {
        private readonly IMoneyService _moneyService;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public PricePartHandler(IMoneyService moneyService, IContentDefinitionManager contentDefinitionManager)
        {
            _moneyService = moneyService;
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override Task InitializingAsync(InitializingContentContext context, PricePart part)
        {
            GetCurrencySelectionMode(part);

            return base.InitializingAsync(context, part);
        }

        public override Task LoadingAsync(LoadContentContext context, PricePart part)
        {
            part.Price = _moneyService.EnsureCurrency(part.Price);

            GetCurrencySelectionMode(part);

            return base.LoadingAsync(context, part);
        }

        private void GetCurrencySelectionMode(PricePart part)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => String.Equals(x.PartDefinition.Name, "PricePart"));
            var currencySelectionMode = contentTypePartDefinition.GetSettings<PricePartSettings>().CurrencySelectionMode;

            part.CurrencySelectionMode = currencySelectionMode;

            if (currencySelectionMode == CurrencySelectionModes.SpecificCurrency)
            {
                part.CurrencyIsoCode = contentTypePartDefinition.GetSettings<PricePartSettings>().CurrencyIsoCode;
            }
        }
    }
}
