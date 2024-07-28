using Microsoft.AspNetCore.Identity;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.ContentManagement;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using System;
using System.Security.Claims;
using System.Text.Json.Nodes;
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
        claimsPrincipal == null ? null : await _userManager.GetUserAsync(claimsPrincipal) as User;

    public async Task AlterUserSettingAsync(User user, string contentType, Func<JsonObject, JsonObject> updateContentItemJson)
    {
        if (user.Properties[contentType] is not JsonObject contentItem)
        {
            user.Properties[contentType] = JObject.FromObject(await _contentManager.NewAsync(contentType));
            contentItem = user.Properties[contentType]!.AsObject();
        }

        user.Properties[contentType] = updateContentItemJson(contentItem);
        await _session.SaveAsync(user);
    }

    public ContentItem GetUserSetting(User user, string contentType) =>
        user?.Properties[contentType]?.ToObject<ContentItem>();
}
