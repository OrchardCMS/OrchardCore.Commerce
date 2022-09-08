using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Activities;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
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
    private readonly IContentManager _contentManager;
    private readonly INotifier _notifier;
    private readonly IPriceService _priceService;
    private readonly IProductService _productService;
    private readonly IShapeFactory _shapeFactory;
    private readonly IEnumerable<IShoppingCartEvents> _shoppingCartEvents;
    private readonly IShoppingCartPersistence _shoppingCartPersistence;
    private readonly IShoppingCartSerializer _shoppingCartSerializer;
    private readonly IWorkflowManager _workflowManager;
    private readonly IHtmlLocalizer<ShoppingCartController> H;

    [SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The shopping cart needs all of them.")]
    public ShoppingCartController(
        INotifier notifier,
        IOrchardServices<ShoppingCartController> services,
        IPriceService priceService,
        IProductService productService,
        IShapeFactory shapeFactory,
        IEnumerable<IShoppingCartEvents> shoppingCartEvents,
        IShoppingCartPersistence shoppingCartPersistence,
        IShoppingCartSerializer shoppingCartSerializer,
        IWorkflowManager workflowManager)
    {
        _contentManager = services.ContentManager.Value;
        _notifier = notifier;
        _priceService = priceService;
        _productService = productService;
        _shapeFactory = shapeFactory;
        _shoppingCartEvents = shoppingCartEvents;
        _shoppingCartPersistence = shoppingCartPersistence;
        _shoppingCartSerializer = shoppingCartSerializer;
        _workflowManager = workflowManager;
        H = services.HtmlLocalizer.Value;
    }

    [HttpGet]
    [Route("cart")]
    public async Task<ActionResult> Index(string shoppingCartId = null)
    {
        var cart = await _shoppingCartPersistence.RetrieveAsync(shoppingCartId);
        var products = await _productService.GetProductDictionaryAsync(cart.Items.Select(line => line.ProductSku));
        var items = await _priceService.AddPricesAsync(cart.Items);

        var lines = await Task.WhenAll(items.Select(async item =>
        {
            var product = products[item.ProductSku];
            var price = _priceService.SelectPrice(item.Prices);
            var metaData = await _contentManager.GetContentItemMetadataAsync(product);
            return new ShoppingCartLineViewModel(attributes: item.Attributes.ToDictionary(attr => attr.AttributeName))
            {
                Quantity = item.Quantity,
                Product = product,
                ProductSku = item.ProductSku,
                ProductName = product.ContentItem.DisplayText,
                UnitPrice = price,
                LinePrice = item.Quantity * price,
                ProductUrl = Url.RouteUrl(metaData.DisplayRouteValues),
            };
        }));

        if (!lines.Any()) return RedirectToAction(nameof(Empty));

        var model = new ShoppingCartViewModel { Id = shoppingCartId };

        model.Headers.AddRange(new[]
        {
            H["Quantity"],
            H["Product"],
            H["Price"],
            H["Action"],
        });

        model.Totals.AddRange(lines
            .GroupBy(viewModel => viewModel.LinePrice.Currency)
            .Select(group => new Amount(group.Sum(viewModel => viewModel.LinePrice.Value), group.Key)));

        foreach (var shoppingCartEvent in _shoppingCartEvents)
        {
            await shoppingCartEvent.LinesDisplayingAsync(model.Headers, lines);
            await shoppingCartEvent.TotalsDisplayingAsync(model.Totals, lines);
        }

        for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            var row = new List<IShape>();
            var line = lines[lineIndex];

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

            model.Lines.Add(row);
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
