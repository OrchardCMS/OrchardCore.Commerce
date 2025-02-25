using Lombiq.HelpfulLibraries.OrchardCore.Validation;
using Lombiq.HelpfulLibraries.OrchardCore.Workflow;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Activities;
using OrchardCore.Commerce.Inventory.Models;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Workflows.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrontendException = Lombiq.HelpfulLibraries.AspNetCore.Exceptions.FrontendException;

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
    private readonly IProductService _productService;

    // These are needed.
#pragma warning disable S107 // Methods should not have too many parameters
    public ShoppingCartController(
        INotifier notifier,
        IShapeFactory shapeFactory,
        IShoppingCartHelpers shoppingCartHelpers,
        IShoppingCartPersistence shoppingCartPersistence,
        IShoppingCartSerializer shoppingCartSerializer,
        IEnumerable<IWorkflowManager> workflowManagers,
        IHtmlLocalizer<ShoppingCartController> htmlLocalizer,
        IEnumerable<IShoppingCartEvents> shoppingCartEvents,
        IProductService productService)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        _notifier = notifier;
        _shapeFactory = shapeFactory;
        _shoppingCartHelpers = shoppingCartHelpers;
        _shoppingCartPersistence = shoppingCartPersistence;
        _shoppingCartSerializer = shoppingCartSerializer;
        _workflowManagers = workflowManagers;
        _shoppingCartEvents = shoppingCartEvents;
        _productService = productService;
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
    [Route("cart/empty")]
    public new async Task<ActionResult> Empty()
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
        var updatedLines = new List<ShoppingCartLineUpdateModel>();

        var lines = await cart.Lines.AwaitEachAsync(async line =>
            (Line: line, Item: await _shoppingCartSerializer.ParseCartLineAsync(line)));

        // Check if there are any line items that failed to deserialize. This can only happen if the shopping cart
        // update model was manually altered or if a product from cart was removed in the backend. This is however
        // unlikely, because products should be made unavailable rather than deleted.
        if (!ModelState.IsValid || lines.Any(line => line.Item == null))
        {
            await _shoppingCartPersistence.StoreAsync(new ShoppingCart(), shoppingCartId);

            await _notifier.ErrorAsync(
                H["Your shopping cart is broken and had to be replaced. We apologize for the inconvenience."]);

            return RedirectToAction(nameof(Empty));
        }

        foreach (var (line, item) in lines)
        {
            var isValid = true;

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
                var minOrderQuantity = (await _productService.GetProductAsync(line.ProductSku))?
                    .As<InventoryPart>()?.MinimumOrderQuantity.Value ?? 0;

                // Choose new quantity based on whether Minimum Order Quantity has a value.
                line.Quantity = (int)(minOrderQuantity > 0 ? minOrderQuantity : 1);
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
            if (!ModelState.IsValid || await _shoppingCartSerializer.ParseCartLineAsync(line) is not { } shoppingCartItem)
            {
                return NotFound();
            }

            var parsedLine = await _shoppingCartHelpers.AddToCartAsync(
                shoppingCartId,
                shoppingCartItem,
                storeIfOk: true);

            await _workflowManagers.TriggerEventAsync<ProductAddedToCartEvent>(
                new { LineItem = parsedLine },
                $"ShoppingCart-{HttpContext.Session.Id}-{shoppingCartId}");
        }
        catch (FrontendException exception)
        {
            await _notifier.FrontEndErrorAsync(exception);
        }

        return RedirectToAction(nameof(Index), new { shoppingCartId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> RemoveItem(ShoppingCartLineUpdateModel line, string shoppingCartId = null)
    {
        if (ModelState.IsValid)
        {
            var parsedLine = await _shoppingCartSerializer.ParseCartLineAsync(line);
            var cart = await _shoppingCartPersistence.RetrieveAsync(shoppingCartId);
            cart.RemoveItem(parsedLine);
            await _shoppingCartPersistence.StoreAsync(cart, shoppingCartId);
        }

        return RedirectToAction(nameof(Index), new { shoppingCartId });
    }
}
