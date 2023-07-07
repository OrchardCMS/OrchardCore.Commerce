using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Extensions;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using YesSql;
using static OrchardCore.Commerce.Constants.ContentTypes;

namespace OrchardCore.Commerce.Controllers;

public class UserController : Controller
{
    private readonly IContentItemDisplayManager _contentItemDisplayManager;
    private readonly IContentManager _contentManager;
    private readonly INotifier _notifier;
    private readonly ISession _session;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly UserManager<IUser> _userManager;

    private readonly IHtmlLocalizer<UserController> H;

    public UserController(
        IContentItemDisplayManager contentItemDisplayManager,
        INotifier notifier,
        IOrchardServices<UserController> services,
        ISession session,
        IUpdateModelAccessor updateModelAccessor)
    {
        _contentItemDisplayManager = contentItemDisplayManager;
        _contentManager = services.ContentManager.Value;
        _notifier = notifier;
        _session = session;
        _updateModelAccessor = updateModelAccessor;
        _userManager = services.UserManager.Value;

        H = services.HtmlLocalizer.Value;
    }

    [HttpGet]
    public async Task<IActionResult> Addresses()
    {
        if (User.Identity?.IsAuthenticated != true) return LocalRedirect("~/Login?ReturnUrl=~/user/addresses");
        if (await _userManager.GetUserAsync(User) is not User user) return NotFound();

        var userAddresses = await GetUserContentItemAsync(user, UserAddresses);
        var editor = await _contentItemDisplayManager.BuildEditorAsync(
            userAddresses,
            _updateModelAccessor.ModelUpdater,
            isNew: false);

        return View(editor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("user/addresses")]
    public async Task<IActionResult> AddressesPost()
    {
        if (User.Identity?.IsAuthenticated != true) return LocalRedirect("~/Login?ReturnUrl=~/user/addresses");
        if (await _userManager.GetUserAsync(User) is not User user) return NotFound();

        var userAddresses = await GetUserContentItemAsync(user, UserAddresses);
        await _contentItemDisplayManager.UpdateEditorAsync(userAddresses, _updateModelAccessor.ModelUpdater, isNew: false);

        if (_updateModelAccessor.ModelUpdater.ModelState.IsValid)
        {
            user.Put(UserAddresses, userAddresses);
            _session.Save(user);
            await _notifier.SuccessAsync(H["Your addresses have been updated."]);
        }
        else
        {
            var errors = _updateModelAccessor.ModelUpdater.GetModelErrorMessages();
            foreach (var error in errors.WhereNot(string.IsNullOrEmpty)) await _notifier.ErrorAsync(H[error]);
        }

        return RedirectToAction(nameof(Addresses));
    }

    [HttpGet]
    public async Task<IActionResult> Details()
    {
        if (User.Identity?.IsAuthenticated != true) return LocalRedirect("~/Login?ReturnUrl=~/user/details");
        if (await _userManager.GetUserAsync(User) is not User user) return NotFound();

        var userDetails = await GetUserContentItemAsync(user, UserDetails);
        var editor = await _contentItemDisplayManager.BuildEditorAsync(
            userDetails,
            _updateModelAccessor.ModelUpdater,
            isNew: false);

        return View(editor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("user/details")]
    public async Task<IActionResult> DetailsPost()
    {
        if (User.Identity?.IsAuthenticated != true) return LocalRedirect("~/Login?ReturnUrl=~/user/details");
        if (await _userManager.GetUserAsync(User) is not User user) return NotFound();

        var userDetails = await GetUserContentItemAsync(user, UserDetails);
        await _contentItemDisplayManager.UpdateEditorAsync(userDetails, _updateModelAccessor.ModelUpdater, isNew: false);

        if (_updateModelAccessor.ModelUpdater.ModelState.IsValid)
        {
            user.Put(UserDetails, userDetails);
            _session.Save(user);
            await _notifier.SuccessAsync(H["Your details have been updated."]);
        }
        else
        {
            var errors = _updateModelAccessor.ModelUpdater.GetModelErrorMessages();
            foreach (var error in errors.WhereNot(string.IsNullOrEmpty)) await _notifier.ErrorAsync(H[error]);
        }

        return RedirectToAction(nameof(Details));
    }

    private async Task<ContentItem> GetUserContentItemAsync(User user, string contentType)
    {
        var contentItem = user.As<ContentItem>(contentType);

        return string.IsNullOrEmpty(contentItem?.ContentType)
            ? await _contentManager.NewAsync(UserAddresses)
            : contentItem;
    }
}
