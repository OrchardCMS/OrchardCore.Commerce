using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using System;
using System.Threading.Tasks;
using YesSql.Sql;
using static Lombiq.HelpfulLibraries.OrchardCore.Contents.ContentFieldEditorEnums.TextFieldEditors;
using static OrchardCore.Commerce.Abstractions.Constants.ContentTypes;

namespace OrchardCore.Commerce.Migrations;

public class SubscriptionMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public SubscriptionMigrations(IContentDefinitionManager contentDefinitionManager) =>
        _contentDefinitionManager = contentDefinitionManager;

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterTypeDefinitionAsync(Subscription, type => type
            .Listable()
            .Creatable()
            .Securable()
            .WithPart(nameof(SubscriptionPart)));

        await _contentDefinitionManager.AlterPartDefinitionAsync<SubscriptionPart>(builder => builder
            .WithField(part => part.Status, field => field
                .WithEditor(nameof(PredefinedList))
                .WithSettings(new TextFieldPredefinedListEditorSettings
                {
                    Editor = EditorOption.Dropdown,
                }))
            .WithField(part => part.IdInPaymentProvider, field => field.WithDisplayName("Id in payment provider"))
            .WithField(part => part.PaymentProviderName, field => field.WithDisplayName("Payment provider name"))
            .WithField(part => part.UserId, field => field
                .WithDisplayName("User Id")
                .WithDescription("The user ID of the subscriber."))
            .WithField(part => part.SerializedMetadata, field => field
                .WithSettings(new TextFieldSettings
                {
                    Hint = "Additional data about the subscription in Dictionary<string,string> JSON serialized form.",
                })
                .WithDisplayName("Additional data")
                .WithDescription("Additional data about the subscription in JSON serialized form."))
            .WithField(part => part.StartDateUtc, field => field
                .WithSettings(new DateTimeFieldSettings { Required = true })
                .WithDisplayName("Start date")
                .WithDescription("The date when the subscription first started."))
            .WithField(part => part.EndDateUtc, field => field
                .WithSettings(new DateTimeFieldSettings { Required = true })
                .WithDisplayName("End date")
                .WithDescription("The date when the subscription ends."))
        );

        await SchemaBuilder.CreateMapIndexTableAsync<SubscriptionPartIndex>(table => table
            .Column<string>(nameof(SubscriptionPartIndex.Status))
            .Column<string>(nameof(SubscriptionPartIndex.IdInPaymentProvider))
            .Column<string>(nameof(SubscriptionPartIndex.PaymentProviderName))
            .Column<string>(nameof(SubscriptionPartIndex.UserId))
            .Column<string>(nameof(SubscriptionPartIndex.SerializedMetadata))
            .Column<DateTime>(nameof(SubscriptionPartIndex.StartDateUtc))
            .Column<DateTime>(nameof(SubscriptionPartIndex.EndDateUtc))
        );

        return 1;
    }
}
