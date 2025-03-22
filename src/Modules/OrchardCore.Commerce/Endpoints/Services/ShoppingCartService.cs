using Lombiq.HelpfulLibraries.AspNetCore.Exceptions;
using Lombiq.HelpfulLibraries.OrchardCore.Workflow;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Abstractions.ViewModels;
using OrchardCore.Commerce.Activities;
using OrchardCore.Commerce.Inventory.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Endpoints;
public class ShoppingCartService : IShoppingCartService
{
    private readonly IShoppingCartHelpers _shoppingCartHelpers;
    private readonly IShoppingCartPersistence _shoppingCartPersistence;
    private readonly IShoppingCartSerializer _shoppingCartSerializer;
    private readonly IEnumerable<IWorkflowManager> _workflowManagers;
    private readonly IHtmlLocalizer<ShoppingCartService> H;
    private readonly IEnumerable<IShoppingCartEvents> _shoppingCartEvents;
    private readonly IProductService _productService;
    public ShoppingCartService(
        IShoppingCartHelpers shoppingCartHelpers,
        IShoppingCartPersistence shoppingCartPersistence,
        IShoppingCartSerializer shoppingCartSerializer,
        IEnumerable<IWorkflowManager> workflowManagers,
        IHtmlLocalizer<ShoppingCartService> htmlLocalizer,
        IEnumerable<IShoppingCartEvents> shoppingCartEvents,
        IProductService productService)
    {
        _shoppingCartHelpers = shoppingCartHelpers;
        _shoppingCartPersistence = shoppingCartPersistence;
        _shoppingCartSerializer = shoppingCartSerializer;
        _workflowManagers = workflowManagers;
        _shoppingCartEvents = shoppingCartEvents;
        _productService = productService;
        H = htmlLocalizer;
    }

    public async Task<string> RemoveLineAsync(ShoppingCartLineUpdateModel line, string shoppingCartId = null)
    {
        string errored = string.Empty;
        try
        {
            var parsedLine = await _shoppingCartSerializer.ParseCartLineAsync(line);
            var cart = await _shoppingCartPersistence.RetrieveAsync(shoppingCartId);
            cart.RemoveItem(parsedLine);
            await _shoppingCartPersistence.StoreAsync(cart, shoppingCartId);
        }
        catch
        {
            errored = H["An error has occurred."].Value;
        }

        return errored;
    }

    public async Task<string> AddItemAsync(ShoppingCartLineUpdateModel line, string token, string shoppingCartId = null)
    {
        string strError = string.Empty;
        if (await _shoppingCartSerializer.ParseCartLineAsync(line) is not { } shoppingCartItem)
        {
            strError = H["Not Found"].Value;
            return strError;
        }

        try
        {
            await AddItemToCartAsync(shoppingCartItem, token, shoppingCartId);
        }
        catch (FrontendException ex)
        {
            var errors = ex.HtmlMessages.Select(error => error.Html());
            strError = string.Join(System.Environment.NewLine, errors);
        }

        return strError;
    }

    public async Task AddItemToCartAsync(ShoppingCartItem shoppingCartItem, string token, string shoppingCartId)
    {
        var parsedLine = await _shoppingCartHelpers.AddToCartAsync(
            shoppingCartId,
            shoppingCartItem,
            storeIfOk: true);

        await _workflowManagers.TriggerEventAsync<ProductAddedToCartEvent>(
            new { LineItem = parsedLine },
            $"ShoppingCart-{token}-{shoppingCartId}");
    }

    public Task<ShoppingCartViewModel> GetAsync(string shoppingCartId = null) =>
        _shoppingCartHelpers.CreateShoppingCartViewModelAsync(shoppingCartId);

    public async Task<string> UpdateAsync(ShoppingCartUpdateModel cart, string token, string shoppingCartId = null)
    {
        string errored = string.Empty;
        var updatedLines = new List<ShoppingCartLineUpdateModel>();

        var lines = await cart.Lines.AwaitEachAsync(async line =>
            (Line: line, Item: await _shoppingCartSerializer.ParseCartLineAsync(line)));

        // Check if there are any line items that failed to deserialize. This can only happen if the shopping cart
        // update model was manually altered or if a product from cart was removed in the backend. This is however
        // unlikely, because products should be made unavailable rather than deleted.
        if (lines.Any(line => line.Item == null))
        {
            await _shoppingCartPersistence.StoreAsync(new ShoppingCart(), shoppingCartId);
            errored = H["Empty. Your shopping cart is broken and had to be replaced. We apologize for the inconvenience."].Value;
            return errored;
        }

        foreach (var (line, item) in lines)
        {
            await _workflowManagers.TriggerEventAsync<CartUpdatedEvent>(
                new { LineItem = item },
                $"ShoppingCart-{token}-{shoppingCartId}");

            var sb = new StringBuilder();
            foreach (var shoppingCartEvent in _shoppingCartEvents.OrderBy(provider => provider.Order))
            {
                if (await shoppingCartEvent.VerifyingItemAsync(item) is { } errorMessage)
                {
                    sb.AppendLine(errorMessage.Value);
                }
            }

            errored = sb.ToString();
            // Preserve invalid lines in the cart, but modify their Quantity values to valid ones.
            if (!string.IsNullOrEmpty(errored))
            {
                var minOrderQuantity = (await _productService.GetProductAsync(line.ProductSku))
                    .As<InventoryPart>().MinimumOrderQuantity.Value;

                // Choose new quantity based on whether Minimum Order Quantity has a value.
                line.Quantity = (int)(minOrderQuantity > 0 ? minOrderQuantity : 1);
            }

            updatedLines.Add(line);
        }

        cart.Lines.SetItems(updatedLines);
        var parsedCart = await _shoppingCartSerializer.ParseCartAsync(cart);
        await _shoppingCartPersistence.StoreAsync(parsedCart, shoppingCartId);

        return errored;
    }
}
