using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using System.Linq;
using static OrchardCore.Commerce.Constants.ContentTypes;

namespace OrchardCore.Commerce.Drivers;

public class OrderContentTypeDefinitionDisplayDriver : ContentTypeDefinitionDisplayDriver
{
    private readonly IContentManager _contentManager;
    private readonly IFieldsOnlyDisplayManager _fieldsOnlyDisplayManager;

    public OrderContentTypeDefinitionDisplayDriver(
        IContentManager contentManager,
        IFieldsOnlyDisplayManager fieldsOnlyDisplayManager)
    {
        _contentManager = contentManager;
        _fieldsOnlyDisplayManager = fieldsOnlyDisplayManager;
    }

    public override IDisplayResult Edit(ContentTypeDefinition contentTypeDefinition) =>
        contentTypeDefinition.Name == Order
            ? Initialize<OrderPartTemplatesViewModel>(
                    "OrderPart_TemplateLinks",
                    async viewModel =>
                    {
                        var links = await _fieldsOnlyDisplayManager.GetFieldTemplateEditorUrlsAsync(
                            await _contentManager.NewAsync(Order),
                            "Checkout");

                        viewModel.TemplateLinks = links.Select(link =>
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
