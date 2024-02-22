using Lombiq.HelpfulLibraries.OrchardCore.Contents;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.Abstractions.Fields;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Settings;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Html.Models;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Title.Models;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using YesSql;
using static OrchardCore.Commerce.Abstractions.Constants.ContentTypes;
using static OrchardCore.Commerce.Abstractions.Constants.OrderStatuses;
using JArray = Newtonsoft.Json.Linq.JArray;

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
                            new ListValueOption { Name = Pending, Value = Pending.HtmlClassify() },
                            new ListValueOption { Name = Ordered, Value = Ordered.HtmlClassify() },
                            new ListValueOption { Name = Shipped, Value = Shipped.HtmlClassify() },
                            new ListValueOption { Name = Arrived, Value = Arrived.HtmlClassify() },
                        ],
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
                .WithField(nameof(OrderPart.IsCorporation), field => field
                    .OfType(nameof(BooleanField))
                    .WithDisplayName("Buyer is a corporation"))
            );

        return 7;
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
                                new() { Name = Ordered, Value = Ordered.HtmlClassify() },
                                new() { Name = Shipped, Value = Shipped.HtmlClassify() },
                                new() { Name = Arrived, Value = Arrived.HtmlClassify() },
                            ],
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
                            new ListValueOption { Name = Pending, Value = Pending.HtmlClassify() },
                            new ListValueOption { Name = Ordered, Value = Ordered.HtmlClassify() },
                            new ListValueOption { Name = Shipped, Value = Shipped.HtmlClassify() },
                            new ListValueOption { Name = Arrived, Value = Arrived.HtmlClassify() },
                        ],
                        DefaultValue = Pending.HtmlClassify(),
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

        return 6;
    }

    public int UpdateFrom6()
    {
        ShellScope.AddDeferredTask(async scope =>
        {
            var session = scope.ServiceProvider.GetRequiredService<ISession>();
            var orders = await session.QueryContentItem(PublicationStatus.Any, Order).ListAsync();
            var expectedType = JToken.FromObject(
                new Payment.Models.Payment(
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    Amount.Unspecified,
                    DateTime.UtcNow),
                new JsonSerializer
                {
                    // Same as for <see cref="OrderPart.Charges"/>. We need to use different type name handling other
                    // than none, otherwise we won't have the $type property in the serialized JSON.
#pragma warning disable CA2326 // Do not use TypeNameHandling values other than None
                    TypeNameHandling = TypeNameHandling.Objects,
#pragma warning restore CA2326 // Do not use TypeNameHandling values other than None
                })["$type"]?.ToString();

            foreach (var order in orders)
            {
                if (order.Content.OrderPart["Charges"] is not JArray charges)
                {
                    continue;
                }

                foreach (var payment in charges)
                {
                    var paymentType = payment["$type"]?.ToString();

                    // We should modify only if the old Payment type or wrongly saved Payment type is present before
                    // this migration was written. There might be other serialized types in the Charges JArray that
                    // implements IPayment which shouldn't be modified. Otherwise continue with the next one.
                    if (paymentType is not "OrchardCore.Commerce.Models.Payment, OrchardCore.Commerce" and
                        not "OrchardCore.Commerce.Models.Payment, OrchardCore.Commerce.Payment")
                    {
                        continue;
                    }

                    payment["$type"] = expectedType;
                    await session.SaveAsync(order);
                }
            }
        });

        return 7;
    }
}
