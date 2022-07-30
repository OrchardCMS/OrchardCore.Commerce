using OrchardCore.Commerce.Constants;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Settings;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Html.Models;
using OrchardCore.Title.Models;
using System.Collections.Generic;
using static OrchardCore.Commerce.Constants.ContentTypes;

namespace OrchardCore.Commerce.Migrations;

/// <summary>
/// Adds the order part to the list of available parts and the address field to the list of available fields.
/// </summary>
public class OrderMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public OrderMigrations(IContentDefinitionManager contentDefinitionManager) =>
        _contentDefinitionManager = contentDefinitionManager;

    public int Create()
    {
        _contentDefinitionManager
            .AlterPartDefinition(nameof(OrderPart), builder => builder
                .Attachable()
                .WithDescription("Makes a content item into an order."));

        _contentDefinitionManager.MigrateFieldSettings<AddressField, AddressPartFieldSettings>();

        _contentDefinitionManager
            .AlterTypeDefinition(Order, type => type
                .Creatable()
                .Listable()
                .Securable()
                .Draftable()
                .Versionable()
                .WithPart(nameof(TitlePart), part => part
                    .WithDescription("The title of the order"))
                .WithPart(nameof(HtmlBodyPart), part => part
                    .WithDisplayName("Annotations")
                    .WithSettings(new ContentTypePartSettings
                    {
                        Editor = "Wysiwyg",
                    })
                )
                .WithPart(nameof(OrderPart)));

        _contentDefinitionManager
            .AlterPartDefinition(nameof(OrderPart), part => part
                .Attachable()
                .WithDescription("Makes a content item into an order.")
                .WithField("OrderId", field => field
                    .OfType(nameof(TextField))
                    .WithDisplayName("Order Id")
                    .WithDescription("The id of the order."))
                .WithField(OrderMigrationConstants.Status, field => field
                    .OfType(nameof(TextField))
                    .WithDisplayName(OrderMigrationConstants.Status)
                    .WithDescription("The status of the order.")
                    .WithEditor("PredefinedList")
                    .WithSettings(new TextFieldPredefinedListEditorSettings
                    {
                        Options = new List<ListValueOption>
                        {
                            new ListValueOption { Name = "Ordered", Value = OrderMigrationConstants.Ordered },
                            new ListValueOption { Name = "Shipped", Value = "shipped" },
                            new ListValueOption { Name = "Arrived", Value = "arrived" },
                        }
                        .ToArray(),

                        DefaultValue = OrderMigrationConstants.Ordered,

                        Editor = EditorOption.Radio,
                    }))
                .WithField("BillingAddress", field => field
                    .OfType(nameof(AddressField))
                    .WithDisplayName("Billing Address")
                    .WithDescription("The address of the party that should be billed for this order."))
                .WithField("ShippingAddress", field => field
                    .OfType(nameof(AddressField))
                    .WithDisplayName("Shipping Address")
                    .WithDescription("The address where the order should be shipped."))
                );

        return 2;
    }

    public int UpdateFrom1()
    {
        _contentDefinitionManager
            .AlterTypeDefinition(Order, type => type
                .Creatable()
                .Listable()
                .Securable()
                .Draftable()
                .Versionable()
                .WithPart(nameof(TitlePart), part => part
                    .WithDescription("The title of the order"))
                .WithPart(nameof(HtmlBodyPart), part => part
                    .WithDisplayName("Annotations")
                    .WithSettings(new ContentTypePartSettings
                    {
                        Editor = "Wysiwyg",
                    })
                )
                .WithPart(nameof(OrderPart)));

        _contentDefinitionManager
            .AlterPartDefinition(nameof(OrderPart), part => part
                .Attachable()
                .WithDescription("Makes a content item into an order.")
                .WithField("OrderId", field => field
                    .OfType(nameof(TextField))
                    .WithDisplayName("Order Id")
                    .WithDescription("The id of the order."))
                .WithField(OrderMigrationConstants.Status, field => field
                    .OfType(nameof(TextField))
                    .WithDisplayName(OrderMigrationConstants.Status)
                    .WithDescription("The status of the order.")
                    .WithEditor("PredefinedList")
                    .WithSettings(new TextFieldPredefinedListEditorSettings
                    {
                        Options = new List<ListValueOption>
                        {
                            new ListValueOption { Name = "Ordered", Value = OrderMigrationConstants.Ordered },
                            new ListValueOption { Name = "Shipped", Value = "shipped" },
                            new ListValueOption { Name = "Arrived", Value = "arrived" },
                        }
                        .ToArray(),

                        DefaultValue = OrderMigrationConstants.Ordered,

                        Editor = EditorOption.Radio,
                    }))
                .WithField("BillingAddress", field => field
                    .OfType(nameof(AddressField))
                    .WithDisplayName("Billing Address")
                    .WithDescription("The address of the party that should be billed for this order."))
                .WithField("ShippingAddress", field => field
                    .OfType(nameof(AddressField))
                    .WithDisplayName("Shipping Address")
                    .WithDescription("The address where the order should be shipped."))
                );

        return 2;
    }
}
