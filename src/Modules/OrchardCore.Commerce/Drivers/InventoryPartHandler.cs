using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Inventory.Models;
using OrchardCore.ContentManagement.Handlers;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers;

public class InventoryPartHandler : ContentPartHandler<InventoryPart>
{
    private readonly IProductAttributeService _productAttributeService;

    public InventoryPartHandler(IProductAttributeService productAttributeService) =>
        _productAttributeService = productAttributeService;

    public override Task UpdatingAsync(UpdateContentContext context, InventoryPart part) =>
        _productAttributeService.UpdateInventoryPartFromProductAttributesAsync(part.ContentItem);
}
