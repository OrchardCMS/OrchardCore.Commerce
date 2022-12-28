using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Activities;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Workflows.Services;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Controllers;

public class ShoppingCartController : Controller
{
    private readonly INotifier _notifier;
    private readonly IPriceService _priceService;
    private readonly IShapeFactory _shapeFactory;
    private readonly IShoppingCartHelpers _shoppingCartHelpers;
    private readonly IShoppingCartPersistence _shoppingCartPersistence;
    private readonly IShoppingCartSerializer _shoppingCartSerializer;
    private readonly IWorkflowManager _workflowManager;
    private readonly IHtmlLocalizer<ShoppingCartController> H;
    private readonly IEnumerable<IShoppingCartEvents> _shoppingCartEvents;

    [SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The shopping cart needs all of them.")]
    public ShoppingCartController(
        INotifier notifier,
        IOrchardServices<ShoppingCartController> services,
        IPriceService priceService,
        IShapeFactory shapeFactory,
        IShoppingCartHelpers shoppingCartHelpers,
        IShoppingCartPersistence shoppingCartPersistence,
        IShoppingCartSerializer shoppingCartSerializer,
        IWorkflowManager workflowManager,
        IEnumerable<IShoppingCartEvents> shoppingCartEvents)
    {
        _notifier = notifier;
        _priceService = priceService;
        _shapeFactory = shapeFactory;
        _shoppingCartHelpers = shoppingCartHelpers;
        _shoppingCartPersistence = shoppingCartPersistence;
        _shoppingCartSerializer = shoppingCartSerializer;
        _workflowManager = workflowManager;
        _shoppingCartEvents = shoppingCartEvents;
        H = services.HtmlLocalizer.Value;
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
    public IActionResult Empty() => View();

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
        var parsedLine = await _shoppingCartSerializer.ParseCartLineAsync(line);

        var verificationResults = new List<bool>();
        foreach (var shoppingCartEvent in _shoppingCartEvents.OrderBy(provider => provider.Order))
        {
            verificationResults.Add(await shoppingCartEvent.VerifyingItemAsync(parsedLine));
        }

        if (verificationResults.Any(result => !result))
        {
            await _notifier.ErrorAsync(H["Could not add item to cart due to invalid inventory operation."]);
            return RedirectToAction(nameof(Index), new { shoppingCartId });
        }

        if (await ShoppingCartItem.GetErrorAsync(line.ProductSku, parsedLine, H, _priceService) is { } error)
        {
            await _notifier.ErrorAsync(error);
        }

        if (parsedLine == null) return RedirectToAction(nameof(Index), new { shoppingCartId });

        var cart = await _shoppingCartPersistence.RetrieveAsync(shoppingCartId);
        cart.AddItem(parsedLine);
        await _shoppingCartPersistence.StoreAsync(cart, shoppingCartId);
        if (_workflowManager != null)
        {
            await _workflowManager.TriggerEventAsync(
                nameof(ProductAddedToCartEvent),
                new { LineItem = parsedLine },
                "ShoppingCart-" + _shoppingCartPersistence.GetUniqueCartId(shoppingCartId));
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
