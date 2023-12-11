using Lombiq.HelpfulLibraries.OrchardCore.Data;
using Microsoft.Data.SqlClient;
using OrchardCore.Commerce.Payment.Stripe.Indexes;
using OrchardCore.Commerce.Payment.Stripe.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using YesSql.Sql;
using static OrchardCore.Commerce.Abstractions.Constants.ContentTypes;

namespace OrchardCore.Commerce.Payment.Stripe.Migrations;

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
        try
        {
            SchemaBuilder.DropMapIndexTable(typeof(OrderPaymentIndex));
        }
        catch (SqlException)
        {
            // This is fine, it just means that the table didn't exist.
        }

        SchemaBuilder
            .CreateMapIndexTable<OrderPaymentIndex>(table => table
                .Column<string>(nameof(OrderPaymentIndex.OrderId), column => column.WithCommonUniqueIdLength())
                .Column<string>(nameof(OrderPaymentIndex.PaymentIntentId)));

        return 1;
    }
}
