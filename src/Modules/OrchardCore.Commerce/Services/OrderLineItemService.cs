using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Promotion.Extensions;
using OrchardCore.Commerce.Promotion.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class OrderLineItemService : IOrderLineItemService
{
    private readonly IClock _clock;
    private readonly IHttpContextAccessor _hca;
    private readonly IProductService _productService;
    private readonly IContentManager _contentManager;
    private readonly IEnumerable<ITaxProvider> _taxProviders;
    private readonly IPromotionService _promotionService;

    public OrderLineItemService(
        IClock clock,
        IHttpContextAccessor hca,
        IProductService productService,
        IContentManager contentManager,
        IEnumerable<ITaxProvider> taxProviders,
        IPromotionService promotionService)
    {
        _clock = clock;
        _hca = hca;
        _productService = productService;
        _contentManager = contentManager;
        _taxProviders = taxProviders;
        _promotionService = promotionService;
    }

    public async Task<(IList<OrderLineItemViewModel> ViewModels, Amount Total)> CreateOrderLineItemViewModelsAndTotalAsync(
        IList<OrderLineItem> lineItems,
        OrderPart orderPart)
    {
        if (!lineItems.Any()) return (Array.Empty<OrderLineItemViewModel>(), Amount.Unspecified);

        var products = await _productService.GetProductDictionaryByContentItemVersionsAsync(
            lineItems.Select(line => line.ContentItemVersion));

        var viewModelLineItems = await Task.WhenAll(lineItems.Select(async lineItem =>
        {
            var product = products[lineItem.ProductSku];
            var metaData = await _contentManager.GetContentItemMetadataAsync(product);

            return new OrderLineItemViewModel
            {
                ProductPart = product,
                Quantity = lineItem.Quantity,
                ProductSku = lineItem.ProductSku,
                ProductName = product.ContentItem.DisplayText,
                UnitPriceValue = lineItem.UnitPrice.Value,
                UnitPriceCurrencyIsoCode = lineItem.UnitPrice.Currency.CurrencyIsoCode,
                UnitPrice = lineItem.UnitPrice,
                LinePrice = lineItem.LinePrice,
                ProductRouteValues = metaData.DisplayRouteValues,
                Attributes = lineItem.Attributes,
            };
        }));

        var (shipping, billing) = await _hca.GetUserAddressIfNullAsync(
            orderPart?.ShippingAddress.Address,
            orderPart?.BillingAddress.Address);

        var storedDiscounts = orderPart?.AdditionalData.GetDiscountsByProduct();
        var promotionAndTaxContext = new PromotionAndTaxProviderContext(
            viewModelLineItems.Select(item => new PromotionAndTaxProviderContextLineItem(
                products[item.ProductSku],
                item.UnitPrice,
                item.Quantity,
                GetDiscounts(storedDiscounts, item))),
            viewModelLineItems.CalculateTotals().ToList(),
            shipping,
            billing,
            orderPart?.ContentItem?.PublishedUtc ?? _clock.UtcNow,
            VatNumber: orderPart?.VatNumber.Text,
            Stored: true,
            IsCorporation: orderPart?.IsCorporation.Value ?? false);
        var changed = false;

        if (_taxProviders.Any() &&
            await _taxProviders.GetFirstApplicableProviderAsync(promotionAndTaxContext) is { } taxProvider)
        {
            promotionAndTaxContext = await taxProvider.UpdateAsync(promotionAndTaxContext);
            changed = true;
        }

        if (await _promotionService.IsThereAnyApplicableProviderAsync(promotionAndTaxContext))
        {
            promotionAndTaxContext = await _promotionService.AddPromotionsAsync(promotionAndTaxContext);
            changed = true;
        }

        if (changed)
        {
            foreach (var (item, index) in promotionAndTaxContext.Items.Select((item, index) => (item, index)))
            {
                var lineItem = viewModelLineItems[index];
                lineItem.LinePrice = item.Subtotal;
                lineItem.UnitPrice = item.UnitPrice;
            }
        }

        var total = viewModelLineItems.CalculateTotals().Any()
            ? viewModelLineItems.CalculateTotals().Single()
            : new Amount(0, lineItems[0].LinePrice.Currency);

        return (viewModelLineItems, total);
    }

    private static IEnumerable<DiscountInformation> GetDiscounts(
        IDictionary<string, IEnumerable<DiscountInformation>> storedDiscounts,
        OrderLineItemViewModel item)
    {
        var discounts = storedDiscounts?.GetMaybe(item.ProductSku)?.AsList();
        return discounts?.Any() != true
            ? item.ProductPart.GetAllDiscountInformation()
            : discounts;
    }
}
