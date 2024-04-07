using Microsoft.AspNetCore.Mvc;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.Exactly.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Settings;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Exactly.Services;

public class ExactlyPaymentProvider : IPaymentProvider
{
    public const string ProviderName = "Exactly";

    private readonly IExactlyService _exactlyService;
    private readonly ISiteService _siteService;

    public string Name => ProviderName;

    public ExactlyPaymentProvider(
        IExactlyService exactlyService,
        ISiteService siteService)
    {
        _exactlyService = exactlyService;
        _siteService = siteService;
    }

    public async Task<object> CreatePaymentProviderDataAsync(IPaymentViewModel model)
    {
        var settings = (await _siteService.GetSiteSettingsAsync())?.As<ExactlySettings>();
        return string.IsNullOrEmpty(settings?.ApiKey) || string.IsNullOrEmpty(settings.ProjectId) ? null : new object();
    }

    public Task<IActionResult> UpdateAndRedirectToFinishedOrderAsync(
        Controller controller,
        ContentItem order,
        string shoppingCartId) =>
        throw new System.NotImplementedException();
}
