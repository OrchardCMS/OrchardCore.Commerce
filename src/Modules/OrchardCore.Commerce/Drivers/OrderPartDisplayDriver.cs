using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType.Extensions;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers;

public class OrderPartDisplayDriver : ContentPartDisplayDriver<OrderPart>
{
    private readonly IContentManager _contentManager;
    private readonly IHttpContextAccessor _hca;
    private readonly IProductService _productService;
    private readonly IEnumerable<ITaxProvider> _taxProviders;

    public OrderPartDisplayDriver(
        IContentManager contentManager,
        IHttpContextAccessor hca,
        IProductService productService,
        IEnumerable<ITaxProvider> taxProviders)
    {
        _contentManager = contentManager;
        _hca = hca;
        _productService = productService;
        _taxProviders = taxProviders;
    }

    public override IDisplayResult Display(OrderPart part, BuildPartDisplayContext context) =>
        Initialize<OrderPartViewModel>(GetDisplayShapeType(context), viewModel => PopulateViewModelAsync(viewModel, part))
            .Location("Detail", "Content:25")
            .Location("Summary", "Meta:10");

    public override IDisplayResult Edit(OrderPart part, BuildPartEditorContext context) =>
        IsFrontEnd()
            ? null
            : Initialize<OrderPartViewModel>(
                GetEditorShapeType(context),
                viewModel => PopulateViewModelAsync(viewModel, part));

    public override async Task<IDisplayResult> UpdateAsync(OrderPart part, IUpdateModel updater, UpdatePartEditorContext context)
    {
        // There is no need to show the line items editor in the front end, as the user should only edit that in the
        // cart rather than the order.
        if (_hca.IsCheckoutFrontEnd()) return await EditAsync(part, context);

        var viewModel = new OrderPartViewModel();

        await updater.TryUpdateModelAsync(viewModel, Prefix);

        var lineItems = part.LineItems
             .Where(lineItem => lineItem != null)
             .Select((lineItem, index) =>
             {
                 var quantity = viewModel.LineItems[index].Quantity;
                 lineItem.Quantity = quantity;
                 lineItem.LinePrice = quantity * lineItem.UnitPrice;

                 return lineItem;
             })
             .Where(lineItem => lineItem.Quantity != 0)
             .ToList();

        part.LineItems.Clear();
        part.LineItems.AddRange(lineItems);

        return await EditAsync(part, context);
    }

    private async ValueTask PopulateViewModelAsync(OrderPartViewModel model, OrderPart part)
    {
        model.ContentItem = part.ContentItem;
        var products = await _productService.GetProductDictionaryAsync(part.LineItems.Select(line => line.ProductSku));
        var lineItems = await Task.WhenAll(part.LineItems.Select(async lineItem =>
        {
            var product = products[lineItem.ProductSku];
            var metaData = await _contentManager.GetContentItemMetadataAsync(product);

            return new OrderLineItemViewModel
            {
                Quantity = lineItem.Quantity,
                ProductSku = lineItem.ProductSku,
                ProductName = product.ContentItem.DisplayText,
                UnitPrice = lineItem.UnitPrice,
                LinePrice = lineItem.LinePrice,
                ProductRouteValues = metaData.DisplayRouteValues,
                Attributes = lineItem.Attributes,
            };
        }));

        var total = lineItems.Select(item => item.LinePrice).Sum();

        if (_taxProviders.Any())
        {
            var taxContext = new TaxProviderContext(
                lineItems.Select(item => new TaxProviderContextLineItem(
                    products[item.ProductSku],
                    item.UnitPrice,
                    item.Quantity)),
                new[] { total });

            taxContext = await _taxProviders.UpdateWithFirstApplicableProviderAsync(taxContext);
            total = taxContext.TotalsByCurrency.Single();

            foreach (var (item, index) in taxContext.Items.Select((item, index) => (item, index)))
            {
                var lineItem = lineItems[index];
                lineItem.LinePrice = item.Subtotal;
                lineItem.UnitPrice = item.UnitPrice;
            }
        }

        model.Total = total;
        model.LineItems.AddRange(lineItems);
        model.Charges.AddRange(part.Charges);
        model.OrderPart = part;
    }

    private bool IsFrontEnd() =>
        _hca.HttpContext?.Request.Path.Value?.StartsWithOrdinalIgnoreCase("/checkout") == true ||
        _hca.HttpContext?.Request.Path.Value?.StartsWithOrdinalIgnoreCase("/success") == true;
}
