using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace OrchardCore.Commerce.Migrations;

public class StripeMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public StripeMigrations(IContentDefinitionManager contentDefinitionManager) =>
        _contentDefinitionManager = contentDefinitionManager;

    public int Create()
    {
        _contentDefinitionManager
            .AlterPartDefinition<StripePaymentPart>(builder => builder
                .Configure(part => part.Attachable())
                .WithField(part => part.PaymentIntentId));

        _contentDefinitionManager
            .AlterTypeDefinition(Constants.ContentTypes.Order, builder => builder
                .WithPart(nameof(StripePaymentPart)));

        return 2;
    }

    public int UpdateFrom1()
    {
        _contentDefinitionManager.AlterPartDefinition(
            nameof(StripePaymentPart),
            typeBuilder => typeBuilder.RemoveField("PaymentIntentId"));

        return 2;
    }
}
