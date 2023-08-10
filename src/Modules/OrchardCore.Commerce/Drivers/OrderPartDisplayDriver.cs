using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers;

public class OrderPartDisplayDriver : ContentPartDisplayDriver<OrderPart>
{
    private readonly IOrderLineItemService _orderLineItemService;
    private readonly ICurrencyProvider _currencyProvider;
    private readonly IProductService _productService;
    private readonly IShoppingCartPersistence _shoppingCartPersistence;
    private readonly IStringLocalizer T;
    private readonly IMoneyService _moneyService;

    public OrderPartDisplayDriver(
        IOrderLineItemService orderLineItemService,
        ICurrencyProvider currencyProvider,
        IProductService productService,
        IShoppingCartPersistence shoppingCartPersistence,
        IStringLocalizer<OrderPartDisplayDriver> stringLocalizer,
        IMoneyService moneyService)
    {
        _orderLineItemService = orderLineItemService;
        _currencyProvider = currencyProvider;
        _productService = productService;
        _shoppingCartPersistence = shoppingCartPersistence;
        _moneyService = moneyService;
        T = stringLocalizer;
    }

    public override IDisplayResult Display(OrderPart part, BuildPartDisplayContext context) =>
        Initialize<OrderPartViewModel>(GetDisplayShapeType(context), viewModel => PopulateViewModelAsync(viewModel, part))
            .Location("Detail", "Content:25")
            .Location("Summary", "Meta:10");

    public override IDisplayResult Edit(OrderPart part, BuildPartEditorContext context) =>
        Initialize<OrderPartViewModel>(GetEditorShapeType(context), viewModel => PopulateViewModelAsync(viewModel, part));

    public override async Task<IDisplayResult> UpdateAsync(OrderPart part, IUpdateModel updater, UpdatePartEditorContext context)
    {
        var viewModel = new OrderPartViewModel();
        if (await updater.TryUpdateModelAsync(viewModel, Prefix))
        {
            var viewModelLineItems = viewModel.LineItems
                .Where(lineItem => lineItem != null)
                .Select((lineItem, _) =>
                {
                    var lineItemCurrency = _currencyProvider.GetCurrency(lineItem.UnitPriceCurrencyIsoCode);

                    lineItem.UnitPrice = new Amount(lineItem.UnitPriceValue, lineItemCurrency);
                    lineItem.LinePrice = lineItem.UnitPrice * lineItem.Quantity;

                    return lineItem;
                })
                .Where(lineItem => lineItem.Quantity != 0)
                .ToList();

            var distinctCurrencies = viewModelLineItems.Select(lineItem => lineItem.UnitPriceCurrencyIsoCode).Distinct();

            // If selected currencies don't match, add model error and set prices to 0.
            var currenciesMatch = distinctCurrencies.Count() == 1;
            if (!currenciesMatch)
            {
                updater.ModelState.AddModelError(
                    nameof(viewModel.LineItems),
                    T["Selected currencies need to match."]);
            }

            var orderLineItems = new List<OrderLineItem>();
            foreach (var lineItem in viewModelLineItems)
            {
                var lineItemProductSku = lineItem.ProductSku.ToUpperInvariant();

                // If the provided SKU does not belong to an existing product content item, it should not be added.
                if (await _productService.GetProductAsync(lineItemProductSku) is not { } productPart)
                {
                    continue;
                }

                // get current shopping cart? does it still contain items here?
                    // probably not. Constructing a new item could work though?
                //var shoppingCart = await _shoppingCartPersistence.RetrieveAsync();

                //var currentItem = shoppingCart.Items.FirstOrDefault(item => item.ProductSku == lineItemProductSku);
                // if attributes exist, there must be a full Sku, add it here
                // 
                //var fullSku = lineItem.Attributes.Any() ? _productService.GetOrderFullSku(currentItem, productPart) : string.Empty;
                var fullSku = string.Empty;

                orderLineItems.Add(new OrderLineItem(
                    lineItem.Quantity,
                    lineItemProductSku,
                    fullSku,
                    currenciesMatch
                        ? lineItem.UnitPrice
                        : new Amount(0, _moneyService.DefaultCurrency ?? _currencyProvider.GetCurrency("USD")),
                    currenciesMatch
                        ? lineItem.LinePrice
                        : new Amount(0, _moneyService.DefaultCurrency ?? _currencyProvider.GetCurrency("USD")),
                    productPart.ContentItem.ContentItemVersionId
                ));
            }

            //var lineItems = part.LineItems // use viewModel's LineItems instead of part's LineItems to get the new list
            //     .Where(lineItem => lineItem != null)
            //     .Select((lineItem, index) =>
            //     {
            //         var quantity = viewModel.LineItems[index].Quantity;
            //         lineItem.Quantity = quantity;
            //         lineItem.LinePrice = quantity * lineItem.UnitPrice;

            //         return lineItem;
            //     })
            //     .Where(lineItem => lineItem.Quantity != 0)
            //     .ToList();

            part.LineItems.SetItems(orderLineItems);
        }

        return await EditAsync(part, context);
    }

    private async ValueTask PopulateViewModelAsync(OrderPartViewModel model, OrderPart part)
    {
        model.ContentItem = part.ContentItem;
        var lineItems = part.LineItems;
        var lineItemViewModelsAndTotal = await _orderLineItemService
            .CreateOrderLineItemViewModelsAndTotalAsync(lineItems, part);

        model.Total = lineItemViewModelsAndTotal.Total;
        model.LineItems.AddRange(lineItemViewModelsAndTotal.ViewModels);
        model.Charges.AddRange(part.Charges);
        model.OrderPart = part;
    }
}
