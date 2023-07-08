using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Constants.ContentTypes;

namespace Microsoft.AspNetCore.Identity;

public static class UserManagerExtensions
{
    /// <summary>
    /// Returns the custom user setting of <see cref="UserAddressesPart"/> for the given <paramref name="principal"/>
    /// if the corresponding <see cref="User"/> is not <see langword="null"/>, returns <see langword="null"/> otherwise.
    /// </summary>
    public static async Task<UserAddressesPart> GetUserAddressAsync(this UserManager<IUser> userManager, ClaimsPrincipal principal) =>
        await userManager.GetUserAsync(principal) is User user ? user.As<ContentItem>(UserAddresses)?.As<UserAddressesPart>() : null;

    /// <summary>
    /// Returns the custom user setting of <see cref="UserDetailsPart"/> for the given <paramref name="principal"/>
    /// if the corresponding <see cref="User"/> is not <see langword="null"/>, returns <see langword="null"/> otherwise.
    /// </summary>
    public static async Task<UserDetailsPart> GetUserDetailsAsync(this UserManager<IUser> userManager, ClaimsPrincipal principal) =>
        await userManager.GetUserAsync(principal) is User user ? user.As<ContentItem>(UserDetails)?.As<UserDetailsPart>() : null;
}
