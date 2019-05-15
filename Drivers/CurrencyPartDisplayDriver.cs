using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Commerce.Drivers
{
    public class CurrencyPartDisplayDriver : ContentPartDisplayDriver<CurrencyPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public CurrencyPartDisplayDriver(
            IContentDefinitionManager contentDefinitionManager,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        public override async Task<IDisplayResult> EditAsync(CurrencyPart part, BuildPartEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageCommerceCurrencies))
            {
                return null;
            }

            return Initialize<CurrencyPartViewModel>("CurrencyPart_Edit", model => BuildViewModel(model, part));
        }

        public override async Task<IDisplayResult> UpdateAsync(CurrencyPart part, BuildPartEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageCommerceCurrencies))
            {
                return null;
            }

            var model = new CurrencyPartViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                part.Name = model.Name;
                part.IsoCode = model.IsoCode;
                part.Symbol = model.Symbol;
                part.Culture = model.Culture;
                part.DecimalPlaces = model.DecimalPlaces;

                part.ContentItem.DisplayText = model.Name;
            }

            return await EditAsync(part, context);
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
