using Microsoft.AspNetCore.Components.Forms;
using OrchardCore.Commerce.Inventory.Local.Models;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace OrchardCore.Commerce.Inventory.Local.Migrations;

/// <summary>
/// Adds the inventory part to the list of available parts.
/// </summary>
public class InventoryPartMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public InventoryPartMigrations(IContentDefinitionManager contentDefinitionManager) =>
        _contentDefinitionManager = contentDefinitionManager;

    public int Create()
    {
        _contentDefinitionManager
            .AlterPartDefinition<InventoryPart>(builder => builder
                .Configure(part => part
                    .Attachable()
                    .WithDescription("Adds basic inventory management capabilities to a product."))
                .WithField(part => part.AllowsBackOrder, field => field
                    .WithDisplayName("Allows Back Order")
                    .WithSettings(new BooleanFieldSettings
                    {
                        DefaultValue = false,
                        Hint = "Allows ordering even if Inventory is less than or equal to zero.",
                    })
                )
                .WithField(part => part.IgnoreInventory, field => field
                    .WithDisplayName("Ignore Inventory")
                    .WithSettings(new BooleanFieldSettings
                    {
                        DefaultValue = false,
                        Hint = "Makes it so Inventory is ignored (same as if no InventoryPart was present). Useful for digital products for example.",
                    })
                )
                .WithField(part => part.Inventory, field => field
                    .WithDisplayName("Inventory")
                    .WithSettings(new NumericFieldSettings
                    {
                        Hint = "The number of items in stock.",
                    })
                )
                .WithField(part => part.MaximumOrderQuantity, field => field
                    .WithDisplayName("Maximum Order Quality")
                    .WithSettings(new NumericFieldSettings
                    {
                        Hint = "The maximum number of this item one can order. Ignored if set to zero or a negative value.",
                    })
                )
                .WithField(part => part.MinimumOrderQuantity, field => field
                    .WithDisplayName("Minimum Order Quality")
                    .WithSettings(new NumericFieldSettings
                    {
                        Hint = "The minimum number of this item one can order. Ignored if set to zero or a negative value.",
                    })
                )
                .WithField(part => part.OutOfStockMessage, field => field
                    .WithDisplayName("Out of Stock Message")
                    .WithEditor("Multiline")
                    .WithSettings(new HtmlFieldSettings
                    {
                        Hint = "Enables a specific message for an out of stock product. Can be used to give an ETA.",
                    })
                )
            );

        return 1;
    }
}
