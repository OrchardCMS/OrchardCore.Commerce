using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Constants.ContentTypes;

namespace OrchardCore.Commerce.Controllers;

public class UserController : Controller
{
    private readonly IContentManager _contentManager;
    private readonly IContentItemDisplayManager _contentItemDisplayManager;
    private readonly INotifier _notifier;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly UserManager<IUser> _userManager;

    private readonly IHtmlLocalizer<UserController> H;

    public UserController(
        IContentItemDisplayManager contentItemDisplayManager,
        INotifier notifier,
        IOrchardServices<UserController> services,
        IUpdateModelAccessor updateModelAccessor)
    {
        _contentManager = services.ContentManager.Value;
        _contentItemDisplayManager = contentItemDisplayManager;
        _notifier = notifier;
        _updateModelAccessor = updateModelAccessor;
        _userManager = services.UserManager.Value;

        H = services.HtmlLocalizer.Value;
    }

    [HttpGet]
    public async Task<IActionResult> Addresses()
    {
        if (await _userManager.GetUserAsync(User) is not User user ||
            user.As<ContentItem>(UserAddresses) is not { } userAddresses)
        {
            return NotFound();
        }

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
        if (await _userManager.GetUserAsync(User) is not User user)
        {
            return NotFound();
        }

        var userAddresses = user.As<ContentItem>(UserAddresses) ?? new ContentItem();
        await _contentItemDisplayManager.UpdateEditorAsync(userAddresses, _updateModelAccessor.ModelUpdater, isNew: false);

        await _contentManager.UpdateAsync(userAddresses);
        await _notifier.SuccessAsync(H["Your addresses have been updated."]);

        return RedirectToAction(nameof(Addresses));
    }
}
