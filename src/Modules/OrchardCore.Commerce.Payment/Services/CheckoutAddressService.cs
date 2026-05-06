using OrchardCore.Commerce.Payment.Settings;
using OrchardCore.Commerce.Payment.ViewModels;
using OrchardCore.Settings;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Services;

public class CheckoutAddressService : ICheckoutAddressService
{
    private readonly ISiteService _siteService;

    public CheckoutAddressService(ISiteService siteService) => _siteService = siteService;

    public virtual async Task<bool> ShouldIgnoreAddressAsync(CheckoutViewModel checkoutViewModel) =>
        (await _siteService.GetSiteSettingsAsync())
        .GetOrCreate<CheckoutAddressSettings>()
        .ShouldIgnoreAddress;
}
