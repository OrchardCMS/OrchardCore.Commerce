using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.Models;
using OrchardCore.Users;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http;

public static class HttpContextExtensions
{
    public static Task<UserAddressesPart> GetUserAddressAsync(this HttpContext context) =>
        context.RequestServices.GetRequiredService<UserManager<IUser>>().GetUserAddressAsync(context.User);

    public static Task<UserDetailsPart> GetUserDetailsAsync(this HttpContext context) =>
        context.RequestServices.GetRequiredService<UserManager<IUser>>().GetUserDetailsAsync(context.User);

    public static async Task<(Address Shipping, Address Billing)> GetUserAddressIfNullAsync(
        this HttpContext httpContext,
        Address shipping,
        Address billing)
    {
        if ((shipping == null || billing == null) &&
            httpContext != null &&
            await httpContext.GetUserAddressAsync() is { } userAddresses)
        {
            shipping ??= userAddresses.GetSafeShippingAddress();
            billing ??= userAddresses.GetSafeBillingAddress();
        }

        return (shipping, billing);
    }

    public static Task<(Address Shipping, Address Billing)> GetUserAddressIfNullAsync(
        this IHttpContextAccessor hca,
        Address shipping,
        Address billing) =>
        GetUserAddressIfNullAsync(hca?.HttpContext, shipping, billing);
}
