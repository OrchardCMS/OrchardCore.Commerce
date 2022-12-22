using Lombiq.HelpfulLibraries.OrchardCore.Data;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Settings;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Utilities;
using OrchardCore.Data.Migration;
using OrchardCore.Html.Models;
using OrchardCore.Title.Models;
using System.Collections.Generic;
using YesSql.Sql;
using static OrchardCore.Commerce.Constants.ContentTypes;
using static OrchardCore.Commerce.Constants.OrderStatuses;

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
                    .WithSettings(new ContentTypePartSettings { Editor = "Wysiwyg", })
                )
                .WithPart(nameof(OrderPart)));

        _contentDefinitionManager
            .AlterPartDefinition(nameof(OrderPart), part => part
                .Attachable()
                .WithDescription("Makes a content item into an order.")
                .WithField(nameof(OrderPart.OrderId), field => field
                    .OfType(nameof(TextField))
                    .WithDisplayName("Order Id")
                    .WithDescription("The id of the order."))
                .WithField(nameof(OrderPart.Status), field => field
                    .OfType(nameof(TextField))
                    .WithDisplayName(nameof(OrderPart.Status))
                    .WithDescription("The status of the order.")
                    .WithEditor("PredefinedList")
                    .WithSettings(new TextFieldPredefinedListEditorSettings
                    {
                        Options = new[]
                        {
                            new ListValueOption { Name = Pending, Value = Pending.HtmlClassify() },
                            new ListValueOption { Name = Ordered, Value = Ordered.HtmlClassify() },
                            new ListValueOption { Name = Shipped, Value = Shipped.HtmlClassify() },
                            new ListValueOption { Name = Arrived, Value = Arrived.HtmlClassify() },
                        },
                        DefaultValue = Pending.HtmlClassify(),
                        Editor = EditorOption.Radio,
                    }))
                .WithField(nameof(OrderPart.Email), field => field
                    .OfType(nameof(TextField))
                    .WithDisplayName("E-mail")
                    .WithSettings(new TextFieldSettings { Required = true }))
                .WithField(nameof(OrderPart.Phone), field => field
                    .OfType(nameof(TextField))
                    .WithDisplayName("Phone Number")
                    .WithSettings(new TextFieldSettings { Required = true }))
                .WithField(nameof(OrderPart.BillingAddress), field => field
                    .OfType(nameof(AddressField))
                    .WithDisplayName("Billing Address")
                    .WithDescription("The address of the party that should be billed for this order."))
                .WithField(nameof(OrderPart.ShippingAddress), field => field
                    .OfType(nameof(AddressField))
                    .WithDisplayName("Shipping Address")
                    .WithDescription("The address where the order should be shipped."))
                .WithField(nameof(OrderPart.BillingAndShippingAddressesMatch), field => field
                    .OfType(nameof(BooleanField))
                    .WithDisplayName("Shipping Address and Billing Address are the same"))
            );

        SchemaBuilder
            .CreateMapIndexTable<OrderPaymentIndex>(table => table
                .Column<string>(nameof(OrderPaymentIndex.OrderId), column => column.WithCommonUniqueIdLength())
                .Column<string>(nameof(OrderPaymentIndex.PaymentIntentId)));

        return 5;
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
                    .WithSettings(new ContentTypePartSettings { Editor = "Wysiwyg", })
                )
                .WithPart(nameof(OrderPart)));

        _contentDefinitionManager
            .AlterPartDefinition(nameof(OrderPart), part => part
                .Attachable()
                .WithDescription("Makes a content item into an order.")
                .WithField(nameof(OrderPart.OrderId), field => field
                    .OfType(nameof(TextField))
                    .WithDisplayName("Order Id")
                    .WithDescription("The id of the order."))
                .WithField(nameof(OrderPart.Status), field => field
                    .OfType(nameof(TextField))
                    .WithDisplayName(nameof(OrderPart.Status))
                    .WithDescription("The status of the order.")
                    .WithEditor("PredefinedList")
                    .WithSettings(new TextFieldPredefinedListEditorSettings
                    {
                        Options = new List<ListValueOption>
                            {
                                new() { Name = Ordered, Value = Ordered.HtmlClassify() },
                                new() { Name = Shipped, Value = Shipped.HtmlClassify() },
                                new() { Name = Arrived, Value = Arrived.HtmlClassify() },
                            }
                            .ToArray(),
                        DefaultValue = Pending.HtmlClassify(),
                        Editor = EditorOption.Radio,
                    }))
                .WithField(nameof(OrderPart.BillingAddress), field => field
                    .OfType(nameof(AddressField))
                    .WithDisplayName("Billing Address")
                    .WithDescription("The address of the party that should be billed for this order."))
                .WithField(nameof(OrderPart.ShippingAddress), field => field
                    .OfType(nameof(AddressField))
                    .WithDisplayName("Shipping Address")
                    .WithDescription("The address where the order should be shipped."))
            );

        return 2;
    }

    public int UpdateFrom2()
    {
        _contentDefinitionManager
            .AlterPartDefinition(nameof(OrderPart), part => part
                .WithField(nameof(OrderPart.Email), field => field
                    .OfType(nameof(TextField))
                    .WithDisplayName("E-mail")
                    .WithSettings(new TextFieldSettings { Required = true }))
                .WithField(nameof(OrderPart.Phone), field => field
                    .OfType(nameof(TextField))
                    .WithDisplayName("Phone Number")
                    .WithSettings(new TextFieldSettings { Required = true }))
                .WithField(nameof(OrderPart.Status), field => field
                    .WithSettings(new TextFieldPredefinedListEditorSettings
                    {
                        Options = new[]
                        {
                            new ListValueOption { Name = Pending, Value = Pending.HtmlClassify() },
                            new ListValueOption { Name = Ordered, Value = Ordered.HtmlClassify() },
                            new ListValueOption { Name = Shipped, Value = Shipped.HtmlClassify() },
                            new ListValueOption { Name = Arrived, Value = Arrived.HtmlClassify() },
                        },
                        DefaultValue = Pending.HtmlClassify(),
                        Editor = EditorOption.Radio,
                    }))
            );

        return 3;
    }

    public int UpdateFrom3()
    {
        _contentDefinitionManager
            .AlterPartDefinition(nameof(OrderPart), part => part
                .WithField(nameof(OrderPart.BillingAndShippingAddressesMatch), field => field
                    .OfType(nameof(BooleanField))
                    .WithDisplayName("Shipping Address and Billing Address are the same"))
            );

        return 4;
    }

    public int UpdateFrom4()
    {
        SchemaBuilder
            .CreateMapIndexTable<OrderPaymentIndex>(table => table
                .Column<string>(nameof(OrderPaymentIndex.OrderId), column => column.WithCommonUniqueIdLength())
                .Column<string>(nameof(OrderPaymentIndex.PaymentIntentId)));

        return 5;
    }
}
