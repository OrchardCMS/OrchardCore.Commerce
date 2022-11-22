using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using System.Collections.Generic;
using System.Linq;
using static OrchardCore.Commerce.Constants.ContentTypes;

namespace OrchardCore.Commerce.Drivers;

public class OrderContentTypeDefinitionDisplayDriver : ContentTypeDefinitionDisplayDriver
{
    // The built-in fields are rendered from the Checkout shape.
    private static readonly string[] _excludedShapes =
    {
        "Order_Checkout__StripePaymentPart__PaymentIntentId",
        "Order_Checkout__StripePaymentPart__PaymentMethodId",
        "Order_Checkout__OrderPart__OrderId",
        "Order_Checkout__OrderPart__Status",
        "Order_Checkout__OrderPart__Email",
        "Order_Checkout__OrderPart__Phone",
        "Order_Checkout__OrderPart__BillingAddress",
        "Order_Checkout__OrderPart__ShippingAddress",
        "Order_Checkout__OrderPart__BillingAndShippingAddressesMatch",
    };

    private readonly IContentManager _contentManager;
    private readonly IFieldsOnlyDisplayManager _fieldsOnlyDisplayManager;

    public OrderContentTypeDefinitionDisplayDriver(
        IContentManager contentManager,
        IFieldsOnlyDisplayManager fieldsOnlyDisplayManager)
    {
        _contentManager = contentManager;
        _fieldsOnlyDisplayManager = fieldsOnlyDisplayManager;
    }

    public override IDisplayResult Edit(ContentTypeDefinition model) =>
        model.Name == Order
            ? Initialize<OrderPartTemplatesViewModel>(
                    "OrderPart_TemplateLinks",
                    async viewModel =>
                    {
                        var templateLinks = await _fieldsOnlyDisplayManager
                            .GetFieldTemplateEditorUrlsAsync(await _contentManager.NewAsync(Order), "Checkout");

                        viewModel.TemplateLinks = templateLinks
                            .WhereNot(link => _excludedShapes.Contains(link.ShapeType))
                            .Select(link =>
                            {
                                var displayText = link.Url
                                    .Query
                                    .Split("name=")
                                    .Last()
                                    .Replace("Order_Checkout__", string.Empty)
                                    .Replace("__", " - ");

                                return (link.Url, displayText, link.IsNew);
                            });
                    })
                .Location("Content:last")
            : null;
}
