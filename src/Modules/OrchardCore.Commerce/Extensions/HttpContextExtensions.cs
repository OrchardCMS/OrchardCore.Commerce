using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Commerce.Models;
using OrchardCore.Users;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http;

public static class HttpContextExtensions
{
    public static Task<UserAddressesPart> GetUserAddressAsync(this HttpContext context) =>
        context.RequestServices.GetRequiredService<UserManager<IUser>>().GetUserAddressAsync(context.User);
}
