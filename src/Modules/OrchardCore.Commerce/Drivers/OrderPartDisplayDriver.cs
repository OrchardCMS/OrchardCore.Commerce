using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.Settings;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Helpers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers;

public class OrderPartDisplayDriver : ContentPartDisplayDriver<OrderPart>
{
    private readonly IEnumerable<IProductAttributeProvider> _attributeProviders;
    private readonly IOrderLineItemService _orderLineItemService;
    private readonly ICurrencyProvider _currencyProvider;
    private readonly IProductService _productService;
    private readonly IStringLocalizer T;
    private readonly IMoneyService _moneyService;
    private readonly IPredefinedValuesProductAttributeService _predefinedValuesProductAttributeService;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IProductAttributeService _productAttributeService;

    // These are needed.
#pragma warning disable S107 // Methods should not have too many parameters
    public OrderPartDisplayDriver(
        IEnumerable<IProductAttributeProvider> attributeProviders,
        IOrderLineItemService orderLineItemService,
        ICurrencyProvider currencyProvider,
        IProductService productService,
        IStringLocalizer<OrderPartDisplayDriver> stringLocalizer,
        IMoneyService moneyService,
        IPredefinedValuesProductAttributeService predefinedValuesProductAttributeService,
        IContentDefinitionManager contentDefinitionManager,
        IProductAttributeService productAttributeService)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        _attributeProviders = attributeProviders;
        _orderLineItemService = orderLineItemService;
        _currencyProvider = currencyProvider;
        _productService = productService;
        _moneyService = moneyService;
        _predefinedValuesProductAttributeService = predefinedValuesProductAttributeService;
        _contentDefinitionManager = contentDefinitionManager;
        _productAttributeService = productAttributeService;
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
                .Take(viewModel.LineItems.Count) // Ensures safe indexing in the Select below.
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
            if (!currenciesMatch && viewModelLineItems.Any())
            {
                updater.ModelState.AddModelError(
                    nameof(viewModel.LineItems),
                    T["Selected currencies must match."]);
            }

            var orderLineItems = await GetOrderLineItemsAsync(updater, nameof(viewModel.LineItems), viewModelLineItems, currenciesMatch);
            part.LineItems.SetItems(orderLineItems);
        }

        return await EditAsync(part, context);
    }

    private async Task<List<OrderLineItem>> GetOrderLineItemsAsync(
        IUpdateModel updater,
        string modelErrorKey,
        List<OrderLineItemViewModel> viewModelLineItems,
        bool currenciesMatch)
    {
        var orderLineItems = new List<OrderLineItem>();
        foreach (var lineItem in viewModelLineItems)
        {
            var lineItemProductSku = lineItem.ProductSku?.ToUpperInvariant();

            // If the provided SKU does not belong to an existing Product content item, it should not be added.
            if (string.IsNullOrEmpty(lineItemProductSku) || await _productService.GetProductAsync(lineItemProductSku) is not { } productPart)
            {
                updater.ModelState.AddModelError(
                    modelErrorKey,
                    string.IsNullOrEmpty(lineItemProductSku)
                        ? T["Product's SKU cannot be left empty."]
                        : T["SKU \"{0}\" does not belong to an existing Product.", lineItemProductSku]);

                continue;
            }

            var processedAttributes = ProcessAttributes(lineItem, productPart);
            var attributesList = processedAttributes.AttributesList;

            // If attributes exist, there must be a full SKU.
            var fullSku = string.Empty;
            if (attributesList.Any())
            {
                var item = new ShoppingCartItem(lineItem.Quantity, lineItem.ProductSku, attributesList);
                fullSku = _productService.GetOrderFullSku(item, productPart);
            }

            var selectedAttributes = new Dictionary<string, IDictionary<string, string>>
            {
                { "Text", processedAttributes.SelectedTextAttributes },
                { "Boolean", processedAttributes.SelectedBooleanAttributes },
                { "Numeric", processedAttributes.SelectedNumericAttributes },
            };

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
                productPart.ContentItem.ContentItemVersionId,
                attributesList,
                selectedAttributes,
                processedAttributes.SelectedBooleanAttributes,
                processedAttributes.SelectedNumericAttributes
            ));
        }

        return orderLineItems;
    }

    private void HandleSelectedNumericAttributes(
        IDictionary<string, string> selectedNumericAttributes,
        ProductPart productPart,
        IList<IProductAttributeValue> attributesList)
    {
        var numericAttributesList = _productAttributeService.GetProductAttributeFields(productPart.ContentItem)
            .Where(attr => attr.Field is NumericProductAttributeField)
            .ToList();

        // Construct actual attributes from strings.
        var type = _contentDefinitionManager.GetTypeDefinition(productPart.ContentItem.ContentType);
        foreach (var attribute in numericAttributesList)
        {
            var (attributePartDefinition, attributeFieldDefinition) = GetFieldDefinition(
                type, type.Name + "." + attribute.Name);

            var defaultValue = ((attribute.Settings as NumericProductAttributeFieldSettings).DefaultValue ?? 0)
                .ToString(CultureInfo.InvariantCulture);
            var selectedNumericAttribute = selectedNumericAttributes.FirstOrDefault(keyValuePair => keyValuePair.Key == attribute.Name);
            var selectedValue = selectedNumericAttribute.Value;

            // If selectedValue is null, set default value in selectedNumericAttributes dictionary to display it properly in editor.
            if (string.IsNullOrEmpty(selectedValue))
            {
                selectedNumericAttributes.Remove(selectedNumericAttribute);
                selectedNumericAttributes.Add(selectedNumericAttribute.Key, defaultValue);
            }

            var matchingAttribute = _attributeProviders
                .Select(provider => provider.Parse(
                    attributePartDefinition,
                    attributeFieldDefinition,
                    selectedValue ?? defaultValue))
                .FirstOrDefault(attributeValue => attributeValue != null);

            attributesList.Add(matchingAttribute);
        }
    }

    private void HandleSelectedBooleanAttributes(
        IDictionary<string, string> selectedBooleanAttributes,
        ProductPart productPart,
        IList<IProductAttributeValue> attributesList)
    {
        var booleanAttributesList = _productAttributeService.GetProductAttributeFields(productPart.ContentItem)
            .Where(attr => attr.Field is BooleanProductAttributeField)
            .Select(attr => attr.Name)
            .ToList();

        // Construct actual attributes from strings.
        var type = _contentDefinitionManager.GetTypeDefinition(productPart.ContentItem.ContentType);
        foreach (var attribute in booleanAttributesList)
        {
            var (attributePartDefinition, attributeFieldDefinition) = GetFieldDefinition(
                type, type.Name + "." + attribute);

            // The value is true if the selected boolean attributes list contains the attribute, otherwise false.
            var value = selectedBooleanAttributes.Any(keyValuePair => keyValuePair.Key == attribute);

            var matchingAttribute = _attributeProviders
                .Select(provider => provider.Parse(
                    attributePartDefinition, attributeFieldDefinition, value.ToString()))
                .FirstOrDefault(attributeValue => attributeValue != null);

            attributesList.Add(matchingAttribute);
        }
    }

    private void HandleSelectedTextAttributes(
        IDictionary<string, string> selectedTextAttributes,
        ProductPart productPart,
        IList<IProductAttributeValue> attributesList)
    {
        var predefinedAttributes = _predefinedValuesProductAttributeService
            .GetProductAttributesRestrictedToPredefinedValues(productPart.ContentItem);

        // Predefined attributes must contain the selected attributes.
        var selectedTextAttributesList = predefinedAttributes
            .Where(predefinedAttr => selectedTextAttributes.Any(selectedAttr => selectedAttr.Key.Contains(predefinedAttr.Name)))
            .ToList();

        // Construct actual attributes from strings.
        var type = _contentDefinitionManager.GetTypeDefinition(productPart.ContentItem.ContentType);
        foreach (var attribute in selectedTextAttributesList)
        {
            var (attributePartDefinition, attributeFieldDefinition) = GetFieldDefinition(
                type, type.Name + "." + attribute.Name);

            var predefinedStrings = new List<string>();
            predefinedStrings.AddRange(
                (attribute.Settings as TextProductAttributeFieldSettings).PredefinedValues.Select(value => value.ToString()));

            var value = predefinedStrings.First(
                item => item == selectedTextAttributes.First(keyValuePair => keyValuePair.Key == attribute.Name).Value);

            var matchingAttribute = _attributeProviders
                .Select(provider => provider.Parse(
                    attributePartDefinition, attributeFieldDefinition, value))
                .FirstOrDefault(attributeValue => attributeValue != null);

            attributesList.Add(matchingAttribute);
        }
    }

    private (List<IProductAttributeValue> AttributesList,
        Dictionary<string, string> SelectedTextAttributes,
        Dictionary<string, string> SelectedBooleanAttributes,
        Dictionary<string, string> SelectedNumericAttributes)
        ProcessAttributes(OrderLineItemViewModel lineItem, ProductPart productPart)
    {
        var attributesList = new List<IProductAttributeValue>();
        var selectedTextAttributes = lineItem.SelectedAttributes["Text"] // try get value?
            .Where(keyValuePair => keyValuePair.Value != null)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        if (selectedTextAttributes.Any())
        {
            HandleSelectedTextAttributes(selectedTextAttributes, productPart, attributesList);
        }

        var selectedBooleanAttributes = lineItem.SelectedBooleanAttributes.ToDictionary(
            pair => pair.Key, pair => pair.Value);
        if (selectedBooleanAttributes.Any())
        {
            HandleSelectedBooleanAttributes(selectedBooleanAttributes, productPart, attributesList);
        }

        var selectedNumericAttributes = lineItem.SelectedNumericAttributes.ToDictionary(
            pair => pair.Key, pair => pair.Value);
        if (selectedNumericAttributes.Any())
        {
            HandleSelectedNumericAttributes(selectedNumericAttributes, productPart, attributesList);
        }

        return (attributesList, selectedTextAttributes, selectedBooleanAttributes, selectedNumericAttributes);
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

    private static (ContentTypePartDefinition PartDefinition, ContentPartFieldDefinition FieldDefinition)
        GetFieldDefinition(ContentTypeDefinition type, string attributeName)
    {
        var partAndField = attributeName.Split('.');
        var partName = partAndField[0];
        var fieldName = partAndField[1];

        return type
            .Parts
            .Where(partDefinition => partDefinition.Name == partName)
            .SelectMany(partDefinition => partDefinition
                .PartDefinition
                .Fields
                .Select(fieldDefinition => (PartDefinition: partDefinition, FieldDefinition: fieldDefinition))
                .Where(pair => pair.FieldDefinition.Name == fieldName))
            .FirstOrDefault();
    }
}
