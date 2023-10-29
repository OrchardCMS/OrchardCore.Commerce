using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Events;

public class UserSettingsCheckoutEvents : ICheckoutEvents
{
    private readonly IHttpContextAccessor _hca;
    public UserSettingsCheckoutEvents(IHttpContextAccessor hca) =>
        _hca = hca;

    public async Task OrderCreatingAsync(OrderPart orderPart, string shoppingCartId)
    {
        if (await _hca.HttpContext.GetUserAddressAsync() is { } userAddresses)
        {
            orderPart.BillingAddress.Address = userAddresses.BillingAddress.Address;
            orderPart.ShippingAddress.Address = userAddresses.ShippingAddress.Address;
            orderPart.BillingAndShippingAddressesMatch.Value = userAddresses.BillingAndShippingAddressesMatch.Value;
        }

        if (await _hca.HttpContext.GetUserDetailsAsync() is { } userDetails)
        {
            orderPart.Phone.Text = userDetails.PhoneNumber.Text;
            orderPart.VatNumber.Text = userDetails.VatNumber.Text;
            orderPart.IsCorporation.Value = userDetails.IsCorporation.Value;
        }
    }
}
