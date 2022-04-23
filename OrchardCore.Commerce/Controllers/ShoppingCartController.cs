using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Money;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Activities;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Commerce.Controllers;

public class ShoppingCartController : Controller
{
    private readonly IShoppingCartPersistence _shoppingCartPersistence;
    private readonly IShoppingCartHelpers _shoppingCartHelpers;
    private readonly IProductService _productService;
    private readonly IPriceService _priceService;
    private readonly IPriceSelectionStrategy _priceStrategy;
    private readonly IContentManager _contentManager;
    private readonly IWorkflowManager _workflowManager;
    private readonly INotifier _notifier;
    private readonly IHtmlLocalizer _h;

    public ShoppingCartController(
        IShoppingCartPersistence shoppingCartPersistence,
        IShoppingCartHelpers shoppingCartHelpers,
        IProductService productService,
        IPriceService priceService,
        IPriceSelectionStrategy priceStrategy,
        IContentManager contentManager,
        IWorkflowManager workflowManager,
        INotifier notifier,
        IHtmlLocalizer<ShoppingCartController> localizer)
    {
        _shoppingCartPersistence = shoppingCartPersistence;
        _shoppingCartHelpers = shoppingCartHelpers;
        _productService = productService;
        _priceService = priceService;
        _priceStrategy = priceStrategy;
        _contentManager = contentManager;
        _workflowManager = workflowManager;
        _notifier = notifier;
        _h = localizer;
    }

    [HttpGet]
    [Route("cart")]
    public async Task<ActionResult> Index(string shoppingCartId = null)
    {
        var cart = await _shoppingCartPersistence.Retrieve(shoppingCartId);
        var products =
            await _productService.GetProductDictionary(cart.Items.Select(line => line.ProductSku));
        var items = await _priceService.AddPrices(cart.Items);
        var lines = await Task.WhenAll(items.Select(async item =>
        {
            var product = products[item.ProductSku];
            var price = _priceStrategy.SelectPrice(item.Prices);
            var metaData = await _contentManager.GetContentItemMetadataAsync(product);
            return new ShoppingCartLineViewModel
            {
                Quantity = item.Quantity,
                ProductSku = item.ProductSku,
                ProductName = product.ContentItem.DisplayText,
                UnitPrice = price,
                LinePrice = item.Quantity * price,
                ProductUrl = Url.RouteUrl(metaData.DisplayRouteValues),
                Attributes = item.Attributes.ToDictionary(attr => attr.AttributeName),
            };
        }));
        var model = new ShoppingCartViewModel
        {
            Id = shoppingCartId,
            Lines = lines,
            Totals = lines.GroupBy(l => l.LinePrice.Currency).Select(g => new Amount(g.Sum(l => l.LinePrice.Value), g.Key)),
        };
        return View(model);
    }

    [HttpPost]
    public async Task<ActionResult> Update(ShoppingCartUpdateModel cart, string shoppingCartId)
    {
        var parsedCart = await _shoppingCartHelpers.ParseCart(cart);
        await _shoppingCartPersistence.Store(parsedCart, shoppingCartId);
        return RedirectToAction(nameof(Index), new { shoppingCartId });
    }

    [HttpGet]
    public Task<ShoppingCart> Get(string shoppingCartId = null)
        => _shoppingCartPersistence.Retrieve(shoppingCartId);

    [HttpPost]
    public async Task<ActionResult> AddItem(ShoppingCartLineUpdateModel line, string shoppingCartId = null)
    {
        var parsedLine = await _shoppingCartHelpers.ParseCartLine(line);
        if (parsedLine is null)
        {
            await _notifier.AddAsync(NotifyType.Error, _h["Product with SKU {0} not found.", line.ProductSku]);
            return RedirectToAction(nameof(Index), new { shoppingCartId });
        }

        parsedLine = (await _priceService.AddPrices(new[] { parsedLine })).Single();
        if (!parsedLine.Prices.Any())
        {
            await _notifier.AddAsync(NotifyType.Error, _h["Can't add product {0} because it doesn't have a price.", line.ProductSku]);
            return RedirectToAction(nameof(Index), new { shoppingCartId });
        }

        var cart = await _shoppingCartPersistence.Retrieve(shoppingCartId);
        cart.AddItem(parsedLine);
        await _shoppingCartPersistence.Store(cart, shoppingCartId);
        if (_workflowManager != null)
        {
            await _workflowManager.TriggerEventAsync(
                nameof(ProductAddedToCartEvent),
                new
                {
                    LineItem = parsedLine,
                },
                "ShoppingCart-" + _shoppingCartPersistence.GetUniqueCartId(shoppingCartId));
        }

        return RedirectToAction(nameof(Index), new { shoppingCartId });
    }

    [HttpPost]
    public async Task<ActionResult> RemoveItem(ShoppingCartLineUpdateModel line, string shoppingCartId = null)
    {
        var parsedLine = await _shoppingCartHelpers.ParseCartLine(line);
        var cart = await _shoppingCartPersistence.Retrieve(shoppingCartId);
        cart.RemoveItem(parsedLine);
        await _shoppingCartPersistence.Store(cart, shoppingCartId);
        return RedirectToAction(nameof(Index), new { shoppingCartId });
    }
}
