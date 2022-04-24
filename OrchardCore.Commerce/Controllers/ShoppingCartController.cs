using Microsoft.AspNetCore.Mvc;
using Money;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Activities;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Services;
using System.Linq;
using System.Threading.Tasks;

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

    public ShoppingCartController(
        IShoppingCartPersistence shoppingCartPersistence,
        IShoppingCartHelpers shoppingCartHelpers,
        IProductService productService,
        IPriceService priceService,
        IPriceSelectionStrategy priceStrategy,
        IContentManager contentManager,
        IWorkflowManager workflowManager)
    {
        _shoppingCartPersistence = shoppingCartPersistence;
        _shoppingCartHelpers = shoppingCartHelpers;
        _productService = productService;
        _priceService = priceService;
        _priceStrategy = priceStrategy;
        _contentManager = contentManager;
        _workflowManager = workflowManager;
    }

    [HttpGet]
    [Route("cart")]
    public async Task<ActionResult> Index(string shoppingCartId = null)
    {
        var cart = await _shoppingCartPersistence.RetrieveAsync(shoppingCartId);
        var products =
            await _productService.GetProductDictionaryAsync(cart.Items.Select(line => line.ProductSku));
        var items = await _priceService.AddPricesAsync(cart.Items);
        var lines = await Task.WhenAll(items.Select(async item =>
        {
            var product = products[item.ProductSku];
            var price = _priceStrategy.SelectPrice(item.Prices);
            var metaData = await _contentManager.GetContentItemMetadataAsync(product);
            return new ShoppingCartLineViewModel(
                attributes: item.Attributes.ToDictionary(attr => attr.AttributeName))
            {
                Quantity = item.Quantity,
                ProductSku = item.ProductSku,
                ProductName = product.ContentItem.DisplayText,
                UnitPrice = price,
                LinePrice = item.Quantity * price,
                ProductUrl = Url.RouteUrl(metaData.DisplayRouteValues),
            };
        }));
        var model = new ShoppingCartViewModel(lines)
        {
            Id = shoppingCartId,
            Totals = lines.GroupBy(l => l.LinePrice.Currency).Select(g => new Amount(g.Sum(l => l.LinePrice.Value), g.Key)),
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Update(ShoppingCartUpdateModel cart, string shoppingCartId)
    {
        var parsedCart = await _shoppingCartHelpers.ParseCartAsync(cart);
        await _shoppingCartPersistence.StoreAsync(parsedCart, shoppingCartId);
        return RedirectToAction(nameof(Index), new { shoppingCartId });
    }

    [HttpGet]
    public Task<ShoppingCart> Get(string shoppingCartId = null)
        => _shoppingCartPersistence.RetrieveAsync(shoppingCartId);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> AddItem(ShoppingCartLineUpdateModel line, string shoppingCartId = null)
    {
        var parsedLine = await _shoppingCartHelpers.ValidateParsedCartLineAsync(
            line,
            await _shoppingCartHelpers.ParseCartLineAsync(line));

        if (parsedLine == null) return RedirectToAction(nameof(Index), new { shoppingCartId });

        var cart = await _shoppingCartPersistence.RetrieveAsync(shoppingCartId);
        cart.AddItem(parsedLine);
        await _shoppingCartPersistence.StoreAsync(cart, shoppingCartId);
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
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> RemoveItem(ShoppingCartLineUpdateModel line, string shoppingCartId = null)
    {
        var parsedLine = await _shoppingCartHelpers.ParseCartLineAsync(line);
        var cart = await _shoppingCartPersistence.RetrieveAsync(shoppingCartId);
        cart.RemoveItem(parsedLine);
        await _shoppingCartPersistence.StoreAsync(cart, shoppingCartId);
        return RedirectToAction(nameof(Index), new { shoppingCartId });
    }
}
