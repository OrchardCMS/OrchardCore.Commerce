using OrchardCore.Commerce.Abstractions.Constants;
using OrchardCore.Commerce.Abstractions.Fields;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Settings;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Html.Models;
using OrchardCore.Title.Models;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Abstractions.Constants.ContentTypes;
using static OrchardCore.Commerce.Abstractions.Constants.OrderStatuses;

namespace OrchardCore.Commerce.Migrations;

/// <summary>
/// Adds the order part to the list of available parts and the address field to the list of available fields.
/// </summary>
public class OrderMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public OrderMigrations(IContentDefinitionManager contentDefinitionManager) =>
        _contentDefinitionManager = contentDefinitionManager;

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager
            .AlterPartDefinitionAsync(nameof(OrderPart), builder => builder
                .Attachable()
                .WithDescription("Makes a content item into an order."));

        await _contentDefinitionManager.MigrateFieldSettingsAsync<AddressField, AddressPartFieldSettings>();

        await _contentDefinitionManager
            .AlterTypeDefinitionAsync(Order, type => type
                .Creatable()
                .Listable()
                .Securable()
                .Draftable()
                .Versionable()
                .WithPart(nameof(TitlePart), part => part
                    .WithDescription("The title of the order"))
                .WithPart(nameof(HtmlBodyPart), part => part
                    .WithDisplayName("Annotations")
                    .WithSettings(new ContentTypePartSettings { Editor = "Trumbowyg", })
                )
                .WithPart(nameof(OrderPart)));

        await _contentDefinitionManager
            .AlterPartDefinitionAsync(nameof(OrderPart), part => part
                .Attachable()
                .WithDescription("Makes a content item into an order.")
                .WithField(nameof(OrderPart.OrderId), field => field
                    .OfType(nameof(TextField))
                    .WithDisplayName("Order Id")
                    .WithDescription("The id of the order."))
                .WithField(nameof(OrderPart.VatNumber), field => field
                    .OfType(nameof(TextField))
                    .WithDisplayName("VAT Number")
                    .WithDescription("The VAT number of the buyer, in case it's a corporation."))
                .WithField(nameof(OrderPart.Status), field => field
                    .OfType(nameof(TextField))
                    .WithDisplayName(nameof(OrderPart.Status))
                    .WithDescription("The status of the order.")
                    .WithEditor("PredefinedList")
                    .WithSettings(new TextFieldPredefinedListEditorSettings
                    {
                        Options =
                        [
                            new ListValueOption { Name = Pending, Value = OrderStatusCodes.Pending },
                            new ListValueOption { Name = Ordered, Value = OrderStatusCodes.Ordered },
                            new ListValueOption { Name = Shipped, Value = OrderStatusCodes.Shipped },
                            new ListValueOption { Name = Arrived, Value = OrderStatusCodes.Arrived },
                            new ListValueOption { Name = PaymentFailed, Value = OrderStatusCodes.PaymentFailed },
                        ],
                        DefaultValue = OrderStatusCodes.Pending,
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
                .WithField(nameof(OrderPart.IsCorporation), field => field
                    .OfType(nameof(BooleanField))
                    .WithDisplayName("Buyer is a corporation"))
            );

        return 9;
    }

    public async Task<int> UpdateFrom1Async()
    {
        await _contentDefinitionManager
            .AlterTypeDefinitionAsync(Order, type => type
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

        await _contentDefinitionManager
            .AlterPartDefinitionAsync(nameof(OrderPart), part => part
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
                        Options =
                            [
                                new() { Name = Ordered, Value = OrderStatusCodes.Ordered },
                                new() { Name = Shipped, Value = OrderStatusCodes.Shipped },
                                new() { Name = Arrived, Value = OrderStatusCodes.Arrived },
                            ],
                        DefaultValue = OrderStatusCodes.Pending,
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

    public async Task<int> UpdateFrom2Async()
    {
        await _contentDefinitionManager
            .AlterPartDefinitionAsync(nameof(OrderPart), part => part
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
                        Options =
                        [
                            new ListValueOption { Name = Pending, Value = OrderStatusCodes.Pending },
                            new ListValueOption { Name = Ordered, Value = OrderStatusCodes.Ordered },
                            new ListValueOption { Name = Shipped, Value = OrderStatusCodes.Shipped },
                            new ListValueOption { Name = Arrived, Value = OrderStatusCodes.Arrived },
                        ],
                        DefaultValue = OrderStatusCodes.Pending,
                        Editor = EditorOption.Radio,
                    }))
            );

        return 3;
    }

    public async Task<int> UpdateFrom3Async()
    {
        await _contentDefinitionManager
            .AlterPartDefinitionAsync(nameof(OrderPart), part => part
                .WithField(nameof(OrderPart.BillingAndShippingAddressesMatch), field => field
                    .OfType(nameof(BooleanField))
                    .WithDisplayName("Shipping Address and Billing Address are the same"))
            );

        return 4;
    }

    [SuppressMessage("Minor Code Smell", "S3400:Methods should not return constants", Justification = "Special migration.")]
    public int UpdateFrom4() => 5; // Moved into a separate module.

    public async Task<int> UpdateFrom5Async()
    {
        await _contentDefinitionManager
            .AlterPartDefinitionAsync(nameof(OrderPart), part => part
                .WithField(nameof(OrderPart.IsCorporation), field => field
                    .OfType(nameof(BooleanField))
                    .WithDisplayName("Buyer is a corporation"))
                .WithField(nameof(OrderPart.VatNumber), field => field
                    .OfType(nameof(TextField))
                    .WithDisplayName("VAT Number")
                    .WithDescription("The VAT number of the buyer, in case it's a corporation."))
            );

        return 7;
    }

    // Previously this migration step updated the type indicator property in serialized order payment instances.
    // This is no longer supported due to the intentionally reduced polymorphic deserialization capabilities of
    // System.Text.Json. It's also no longer necessary because OrchardCore.Commerce.Abstractions.Models.Payment is
    // the only accepted item type for OrderPart.Charges.
    [SuppressMessage("Minor Code Smell", "S3400:Methods should not return constants", Justification = "That's not how migrations work.")]
    public int UpdateFrom6() => 7;

    public async Task<int> UpdateFrom7Async()
    {
        await _contentDefinitionManager
            .AlterPartDefinitionAsync(nameof(OrderPart), part => part
                .WithField(nameof(OrderPart.Status), field => field
                    .OfType(nameof(TextField))
                    .WithDisplayName(nameof(OrderPart.Status))
                    .WithDescription("The status of the order.")
                    .WithEditor("PredefinedList")
                    .WithSettings(new TextFieldPredefinedListEditorSettings
                    {
                        Options =
                        [
                            new ListValueOption { Name = Pending, Value = OrderStatusCodes.Pending },
                            new ListValueOption { Name = Ordered, Value = OrderStatusCodes.Ordered },
                            new ListValueOption { Name = Shipped, Value = OrderStatusCodes.Shipped },
                            new ListValueOption { Name = Arrived, Value = OrderStatusCodes.Arrived },
                            new ListValueOption { Name = PaymentFailed, Value = OrderStatusCodes.PaymentFailed },
                        ],
                        DefaultValue = OrderStatusCodes.Pending,
                        Editor = EditorOption.Radio,
                    }))
            );

        return 8;
    }

    public async Task<int> UpdateFrom8Async()
    {
        await _contentDefinitionManager.AlterTypeDefinitionAsync(Order, type => type
            .WithPart<HtmlBodyPart>(part => part
                .WithSettings(new ContentTypePartSettings { Editor = "Trumbowyg", })
            ));

        return 9;
    }
}
