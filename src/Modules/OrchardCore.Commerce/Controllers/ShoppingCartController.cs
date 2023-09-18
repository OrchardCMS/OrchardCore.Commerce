using Lombiq.HelpfulLibraries.OrchardCore.Workflow;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Activities;
using OrchardCore.Commerce.Exceptions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Workflows.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Controllers;

public class ShoppingCartController : Controller
{
    private readonly INotifier _notifier;
    private readonly IShapeFactory _shapeFactory;
    private readonly IShoppingCartHelpers _shoppingCartHelpers;
    private readonly IShoppingCartPersistence _shoppingCartPersistence;
    private readonly IShoppingCartSerializer _shoppingCartSerializer;
    private readonly IEnumerable<IWorkflowManager> _workflowManagers;
    private readonly IHtmlLocalizer<ShoppingCartController> H;
    private readonly IEnumerable<IShoppingCartEvents> _shoppingCartEvents;

    public ShoppingCartController(
        INotifier notifier,
        IShapeFactory shapeFactory,
        IShoppingCartHelpers shoppingCartHelpers,
        IShoppingCartPersistence shoppingCartPersistence,
        IShoppingCartSerializer shoppingCartSerializer,
        IEnumerable<IWorkflowManager> workflowManagers,
        IHtmlLocalizer<ShoppingCartController> htmlLocalizer,
        IEnumerable<IShoppingCartEvents> shoppingCartEvents)
    {
        _notifier = notifier;
        _shapeFactory = shapeFactory;
        _shoppingCartHelpers = shoppingCartHelpers;
        _shoppingCartPersistence = shoppingCartPersistence;
        _shoppingCartSerializer = shoppingCartSerializer;
        _workflowManagers = workflowManagers;
        _shoppingCartEvents = shoppingCartEvents;
        H = htmlLocalizer;
    }

    [HttpGet]
    [Route("cart")]
    public async Task<ActionResult> Index(string shoppingCartId = null)
    {
        if (await _shoppingCartHelpers.CreateShoppingCartViewModelAsync(shoppingCartId) is not { } model)
        {
            return RedirectToAction(nameof(Empty));
        }

        for (var lineIndex = 0; lineIndex < model.Lines.Count; lineIndex++)
        {
            var row = new List<IShape>();
            var line = model.Lines[lineIndex];

            var attributes = line
                .Attributes
                .Values
                .Select((attribute, index) => (Value: attribute, Type: attribute.GetType().Name, Index: index))
                .Where(tuple => tuple.Value.UntypedValue != null)
                .ToList();

            for (var columnIndex = 0; columnIndex < model.Headers.Count; columnIndex++)
            {
                var columnName = model.Headers[columnIndex].Name.HtmlClassify().ToPascalCase('-');
                var cellShape = await _shapeFactory.CreateAsync<ShoppingCartCellViewModel>(
                    "ShoppingCartCell_" + columnName,
                    model =>
                    {
                        model.LineIndex = lineIndex;
                        model.ColumnIndex = columnIndex;
                        model.Line = line;
                        model.ProductAttributes.AddRange(attributes);
                    });
                row.Add(cellShape);
            }

            model.TableShapes.Add(row);
        }

        return View(model);
    }

    [HttpGet]
    [Route("cart-empty")]
    public async Task<ActionResult> Empty()
    {
        var trackingConsentFeature = HttpContext.Features.Get<ITrackingConsentFeature>();
        if (trackingConsentFeature?.CanTrack == false)
        {
            await _notifier.ErrorAsync(H["You have to accept cookies in your browser to add items to your shopping " +
                "cart. If you have privacy-related browser extensions like ad-blockers or cookie blockers, they " +
                "might be preventing the website from setting cookies, try disabling them."]);
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Update(ShoppingCartUpdateModel cart, string shoppingCartId)
    {
        // should IgnoreInventory affect this?

        var updatedLines = new List<ShoppingCartLineUpdateModel>();
        foreach (var line in cart.Lines)
        {
            var isValid = true;
            var item = await _shoppingCartSerializer.ParseCartLineAsync(line);

            await _workflowManagers.TriggerEventAsync<CartUpdatedEvent>(
                new { LineItem = item },
                $"ShoppingCart-{HttpContext.Session.Id}-{shoppingCartId}");

            foreach (var shoppingCartEvent in _shoppingCartEvents.OrderBy(provider => provider.Order))
            {
                if (await shoppingCartEvent.VerifyingItemAsync(item) is { } errorMessage)
                {
                    await _notifier.ErrorAsync(errorMessage);
                    isValid = false;
                }
            }

            // Preserve invalid lines in the cart, but modify their Quantity values to valid ones.
            if (!isValid)
            {
                // If an item has been successfully added to the cart, Quantity at 1 must be valid.
                line.Quantity = 1;
            }

            updatedLines.Add(line);
        }

        cart.Lines.SetItems(updatedLines);
        var parsedCart = await _shoppingCartSerializer.ParseCartAsync(cart);
        await _shoppingCartPersistence.StoreAsync(parsedCart, shoppingCartId);

        return RedirectToAction(nameof(Index), new { shoppingCartId });
    }

    [HttpGet]
    public Task<ShoppingCart> Get(string shoppingCartId = null) =>
        _shoppingCartPersistence.RetrieveAsync(shoppingCartId);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> AddItem(ShoppingCartLineUpdateModel line, string shoppingCartId = null)
    {
        try
        {
            var parsedLine = await _shoppingCartHelpers.AddToCartAsync(
                shoppingCartId,
                await _shoppingCartSerializer.ParseCartLineAsync(line),
                storeIfOk: true);

            await _workflowManagers.TriggerEventAsync<ProductAddedToCartEvent>(
                new { LineItem = parsedLine },
                $"ShoppingCart-{HttpContext.Session.Id}-{shoppingCartId}");
        }
        catch (FrontendException exception)
        {
            await _notifier.ErrorAsync(exception.HtmlMessage);
        }

        return RedirectToAction(nameof(Index), new { shoppingCartId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> RemoveItem(ShoppingCartLineUpdateModel line, string shoppingCartId = null)
    {
        var parsedLine = await _shoppingCartSerializer.ParseCartLineAsync(line);
        var cart = await _shoppingCartPersistence.RetrieveAsync(shoppingCartId);
        cart.RemoveItem(parsedLine);
        await _shoppingCartPersistence.StoreAsync(cart, shoppingCartId);
        return RedirectToAction(nameof(Index), new { shoppingCartId });
    }
}
