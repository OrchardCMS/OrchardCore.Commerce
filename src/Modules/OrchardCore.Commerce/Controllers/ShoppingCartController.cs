using Fluid.Parser;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    private readonly IWorkflowManager _workflowManager;
    private readonly IHtmlLocalizer<ShoppingCartController> H;

    public ShoppingCartController(
        INotifier notifier,
        IShapeFactory shapeFactory,
        IShoppingCartHelpers shoppingCartHelpers,
        IShoppingCartPersistence shoppingCartPersistence,
        IShoppingCartSerializer shoppingCartSerializer,
        IWorkflowManager workflowManager,
        IHtmlLocalizer<ShoppingCartController> htmlLocalizer)
    {
        _notifier = notifier;
        _shapeFactory = shapeFactory;
        _shoppingCartHelpers = shoppingCartHelpers;
        _shoppingCartPersistence = shoppingCartPersistence;
        _shoppingCartSerializer = shoppingCartSerializer;
        _workflowManager = workflowManager;
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
            await _notifier.ErrorAsync(H["You have to accept the cookies, to add items to your shopping cart!"]);
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Update(ShoppingCartUpdateModel cart, string shoppingCartId)
    {
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

            if (_workflowManager != null)
            {
                await _workflowManager.TriggerEventAsync(
                    nameof(ProductAddedToCartEvent),
                    new { LineItem = parsedLine },
                    "ShoppingCart-" + _shoppingCartPersistence.GetUniqueCartId(shoppingCartId));
            }
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
