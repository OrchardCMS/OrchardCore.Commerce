using Lombiq.HelpfulLibraries.OrchardCore.Data;
using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using YesSql.Sql;
using static OrchardCore.Commerce.Abstractions.Constants.ContentTypes;

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
            .AlterTypeDefinition(Order, builder => builder
                .WithPart(nameof(StripePaymentPart)));

        // This table may exist when migrating from an old version of the DB where it was created in a different module.
        SchemaBuilder.DropMapIndexTable(typeof(OrderPaymentIndex));
        SchemaBuilder
            .CreateMapIndexTable<OrderPaymentIndex>(table => table
                .Column<string>(nameof(OrderPaymentIndex.OrderId), column => column.WithCommonUniqueIdLength())
                .Column<string>(nameof(OrderPaymentIndex.PaymentIntentId)));

        return 1;
    }
}
