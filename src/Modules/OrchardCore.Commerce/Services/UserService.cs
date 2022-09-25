using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.ContentManagement;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Commerce.Services;

public class UserService : IUserService
{
    private readonly IContentManager _contentManager;
    private readonly ISession _session;
    private readonly UserManager<IUser> _userManager;

    public UserService(
        IContentManager contentManager,
        ISession session,
        UserManager<IUser> userManager)
    {
        _contentManager = contentManager;
        _session = session;
        _userManager = userManager;
    }

    public async Task<User> GetFullUserAsync(ClaimsPrincipal claimsPrincipal) =>
        await _userManager.GetUserAsync(claimsPrincipal) as User;

    public async Task AlterUserSettingAsync(User user, string contentType, Func<JObject, JObject> updateContentItemJson)
    {
        if (user.Properties.GetJObject(contentType) is not { } contentItem)
        {
            user.Properties[contentType] = JObject.FromObject(await _contentManager.NewAsync(contentType));
            contentItem = (JObject)user.Properties[contentType];
        }

        user.Properties[contentType] = updateContentItemJson(contentItem);
        _session.Save(user);
    }
}
