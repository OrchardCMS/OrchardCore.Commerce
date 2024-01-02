using Lombiq.HelpfulLibraries.OrchardCore.Contents;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.Abstractions.Fields;
using OrchardCore.Commerce.Abstractions.Models;
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using YesSql;
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
                .WithField(nameof(OrderPart.IsCorporation), field => field
                    .OfType(nameof(BooleanField))
                    .WithDisplayName("Buyer is a corporation"))
            );

        return 7;
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

    [SuppressMessage("Minor Code Smell", "S3400:Methods should not return constants", Justification = "Special migration.")]
    public int UpdateFrom4() => 5; // Moved into a separate module.

    public int UpdateFrom5()
    {
        _contentDefinitionManager
            .AlterPartDefinition(nameof(OrderPart), part => part
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

            foreach (var order in orders)
            {
                if (order.Content.OrderPart["Charges"] is not { } charges)
                {
                    continue;
                }

                foreach (JObject payment in charges)
                {
                    var paymentType = payment["$type"]?.ToString();
                    if (paymentType is not "OrchardCore.Commerce.Models.Payment, OrchardCore.Commerce" and
                        not "OrchardCore.Commerce.Models.Payment, OrchardCore.Commerce.Payment")
                    {
                        continue;
                    }

                    payment["$type"] = "OrchardCore.Commerce.Payment.Models.Payment, OrchardCore.Commerce.Payment";
                    session.Save(order);
                }
            }
        });

        return 7;
    }
}
