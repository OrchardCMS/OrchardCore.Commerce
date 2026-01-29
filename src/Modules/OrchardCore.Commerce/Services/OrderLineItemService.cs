using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Promotion.Extensions;
using OrchardCore.Commerce.Promotion.Models;
using OrchardCore.Commerce.Settings;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;
using ISession = YesSql.ISession;

namespace OrchardCore.Commerce.Services;

public class OrderLineItemService : IOrderLineItemService
{
    private readonly IClock _clock;
    private readonly IProductService _productService;
    private readonly IContentManager _contentManager;
    private readonly IEnumerable<ITaxProvider> _taxProviders;
    private readonly IPromotionService _promotionService;
    private readonly ISession _session;
    private readonly IProductAttributeService _productAttributeService;
    private readonly IPredefinedValuesProductAttributeService _predefinedAttributeService;
    private readonly IOrchardHelper _orchardHelper;

#pragma warning disable S107 // Methods should not have too many parameters
    public OrderLineItemService(
        IClock clock,
        IProductService productService,
        IContentManager contentManager,
        IEnumerable<ITaxProvider> taxProviders,
        IPromotionService promotionService,
        ISession session,
        IProductAttributeService productAttributeService,
        IPredefinedValuesProductAttributeService predefinedAttributeService,
        IOrchardHelper orchardHelper)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        _clock = clock;
        _productService = productService;
        _contentManager = contentManager;
        _taxProviders = taxProviders;
        _promotionService = promotionService;
        _session = session;
        _productAttributeService = productAttributeService;
        _predefinedAttributeService = predefinedAttributeService;
        _orchardHelper = orchardHelper;
    }

    public async Task<(IList<OrderLineItemViewModel> ViewModels, Amount Total)> CreateOrderLineItemViewModelsAndTotalAsync(
        IList<OrderLineItem> lineItems,
        OrderPart orderPart)
    {
        if (lineItems.Count == 0 || lineItems.All(item => item.ContentItemVersion == null))
        {
            return (Array.Empty<OrderLineItemViewModel>(), Amount.Unspecified);
        }

        var products = await _productService.GetProductDictionaryByContentItemVersionsAsync(
            lineItems.Select(line => line.ContentItemVersion));

        var viewModelLineItems = new List<OrderLineItemViewModel>();
        foreach (var lineItem in lineItems)
        {
            viewModelLineItems.Add(await GetOrderLineItemViewModelAsync(products, lineItem));
        }

        var (shipping, billing) = await _orchardHelper.HttpContext.GetUserAddressIfNullAsync(
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

    public async Task<(Dictionary<string, IDictionary<string, List<string>>> AvailableTextAttributes,
        Dictionary<string, List<string>> AvailableBooleanAttributes,
        Dictionary<string, List<string>> AvailableNumericAttributes,
        Dictionary<string, IDictionary<string, NumericProductAttributeFieldSettings>> NumericAttributeSettings)>
        GetAvailableAttributesAndSettingsAsync()
    {
        var availableTextAttributes = new Dictionary<string, IDictionary<string, List<string>>>();
        var availableBooleanAttributes = new Dictionary<string, List<string>>();
        var availableNumericAttributes = new Dictionary<string, List<string>>();
        var numericAttributeSettings = new Dictionary<string, IDictionary<string, NumericProductAttributeFieldSettings>>();

        var allProducts = await _session.Query<ContentItem, ProductPartIndex>().ListAsync();
        foreach (var product in allProducts)
        {
            var productSku = product.As<ProductPart>().Sku;

            var booleanAttributes = (await _productAttributeService.GetProductAttributeFieldsAsync(product))
                .Where(attribute => attribute.Field is BooleanProductAttributeField)
                .Select(attribute => attribute.Name)
                .ToList();

            if (booleanAttributes.Count != 0)
            {
                availableBooleanAttributes.Add(productSku, booleanAttributes);
            }

            var numericAttributes = (await _productAttributeService.GetProductAttributeFieldsAsync(product))
                .Where(attribute => attribute.Field is NumericProductAttributeField)
                .ToList();

            if (numericAttributes.Count != 0)
            {
                availableNumericAttributes.Add(productSku, numericAttributes.Select(attribute => attribute.Name).ToList());
                numericAttributeSettings.Add(
                    productSku,
                    numericAttributes.ToDictionary(
                        attribute => attribute.Name,
                        attribute => attribute.Settings as NumericProductAttributeFieldSettings));
            }

            var textAttributes = await _predefinedAttributeService.GetProductAttributesRestrictedToPredefinedValuesAsync(product);
            foreach (var attribute in textAttributes)
            {
                var settings = (TextProductAttributeFieldSettings)attribute.Settings;
                var predefinedStrings = settings.PredefinedValues.Select(value => value.ToString()).ToList();

                var attributesValuesByAttributeNames = new Dictionary<string, List<string>>
                {
                    { attribute.Name, predefinedStrings },
                };

                // A Product may already be present in the dictionary, but if it has several different attributes,
                // those still need to be added to its key.
                if (!availableTextAttributes.TryAdd(productSku, attributesValuesByAttributeNames))
                {
                    availableTextAttributes[productSku].Add(attribute.Name, predefinedStrings);
                }
            }
        }

        return (availableTextAttributes, availableBooleanAttributes, availableNumericAttributes, numericAttributeSettings);
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

    private async Task<OrderLineItemViewModel> GetOrderLineItemViewModelAsync(
        IDictionary<string, ProductPart> products,
        OrderLineItem lineItem)
    {
        var productPart = products[lineItem.ProductSku];
        var metaData = await _contentManager.GetContentItemMetadataAsync(productPart);

        var fullSku = lineItem.FullSku;
        if (string.IsNullOrEmpty(lineItem.FullSku))
        {
            var item = new ShoppingCartItem(lineItem.Quantity, lineItem.ProductSku, lineItem.Attributes);
            fullSku = await _productService.GetOrderFullSkuAsync(item, productPart);
        }

        var availableAttributesAndSettings = await GetAvailableAttributesAndSettingsAsync();

        return new OrderLineItemViewModel
        {
            ProductPart = productPart,
            Quantity = lineItem.Quantity,
            ProductSku = lineItem.ProductSku,
            ProductFullSku = fullSku,
            ProductName = productPart.ContentItem.DisplayText,
            UnitPriceValue = lineItem.UnitPrice.Value,
            UnitPriceCurrencyIsoCode = lineItem.UnitPrice.Currency.CurrencyIsoCode,
            UnitPrice = lineItem.UnitPrice,
            LinePrice = lineItem.LinePrice,
            ProductRouteValues = metaData.DisplayRouteValues,
            ProductImageUrl = productPart.ProductImage?.Paths?.FirstOrDefault() is { } productImagePath
                ? _orchardHelper.AssetUrl(productImagePath)
                : null,
            Attributes = lineItem.Attributes,
            SelectedAttributes = lineItem.SelectedAttributes,
            AvailableTextAttributes = availableAttributesAndSettings.AvailableTextAttributes,
            AvailableBooleanAttributes = availableAttributesAndSettings.AvailableBooleanAttributes,
            AvailableNumericAttributes = availableAttributesAndSettings.AvailableNumericAttributes,
            NumericAttributeSettings = availableAttributesAndSettings.NumericAttributeSettings,
        };
    }
}
