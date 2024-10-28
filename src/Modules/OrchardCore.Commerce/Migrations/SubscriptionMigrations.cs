using OrchardCore.Commerce.Models;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using System.Threading.Tasks;
using static Lombiq.HelpfulLibraries.OrchardCore.Contents.ContentFieldEditorEnums.TextFieldEditors;
using static OrchardCore.Commerce.Abstractions.Constants.ContentTypes;
using static OrchardCore.Commerce.Constants.SubscriptionStatuses;

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
                    Options =
                    [
                        new ListValueOption { Name = Active, Value = Active },
                        new ListValueOption { Name = Canceled, Value = Canceled },
                        new ListValueOption { Name = Expired, Value = Expired },
                        new ListValueOption { Name = Pending, Value = Pending },
                        new ListValueOption { Name = Deleted, Value = Deleted },
                    ],
                    Editor = EditorOption.Dropdown,
                }))
            .WithField(part => part.IdInPaymentProvider, field => field.WithDisplayName("Id in payment provider"))
            .WithField(part => part.PaymentProviderName, field => field.WithDisplayName("Payment provider name"))
            .WithField(part => part.UserId, field => field
                .WithDisplayName("User Id")
                .WithDescription("The user ID of the subscriber."))
            .WithField(part => part.StartDateUtc, field => field
                .WithDisplayName("Start date")
                .WithDescription("The date when the subscription started."))
            .WithField(part => part.EndDateUtc, field => field
                .WithDisplayName("End date")
                .WithDescription("The date when the subscription ends."))
        );

        return 1;
    }
}
