using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Activities;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Workflows.Services;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Controllers;

public class ShoppingCartController : Controller
{
    private readonly IContentManager _contentManager;
    private readonly INotifier _notifier;
    private readonly IPriceService _priceService;
    private readonly IProductService _productService;
    private readonly IShoppingCartPersistence _shoppingCartPersistence;
    private readonly IShoppingCartSerializer _shoppingCartSerializer;
    private readonly IWorkflowManager _workflowManager;
    private readonly IHtmlLocalizer<ShoppingCartController> T;

    public ShoppingCartController(
        INotifier notifier,
        IOrchardServices<ShoppingCartController> services,
        IPriceService priceService,
        IProductService productService,
        IShoppingCartPersistence shoppingCartPersistence,
        IShoppingCartSerializer shoppingCartSerializer,
        IWorkflowManager workflowManager)
    {
        _contentManager = services.ContentManager.Value;
        _notifier = notifier;
        _priceService = priceService;
        _productService = productService;
        _shoppingCartPersistence = shoppingCartPersistence;
        _shoppingCartSerializer = shoppingCartSerializer;
        _workflowManager = workflowManager;
        T = services.HtmlLocalizer.Value;
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
            Totals = lines
                .GroupBy(viewModel => viewModel.LinePrice.Currency)
                .Select(group => new Amount(group.Sum(viewModel => viewModel.LinePrice.Value), group.Key)),
        };
        return View(model);
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
        var parsedLine = await _shoppingCartSerializer.ParseCartLineAsync(line);

        if (await ShoppingCartItem.GetErrorAsync(line.ProductSku, parsedLine, T, _priceService) is { } error)
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
