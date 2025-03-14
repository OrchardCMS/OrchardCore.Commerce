using OrchardCore.Commerce.Payment.Stripe.Indexes;
using OrchardCore.Data.Migration;
using System.Threading.Tasks;
using YesSql.Sql;

namespace OrchardCore.Commerce.Payment.Stripe.Migrations;

public class StripeSessionMigrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await SchemaBuilder
            .CreateMapIndexTableAsync<StripeSessionDataIndex>(table => table
                .Column<string>(nameof(StripeSessionDataIndex.UserId))
                .Column<string>(nameof(StripeSessionDataIndex.StripeCustomerId))
                .Column<string>(nameof(StripeSessionDataIndex.StripeSessionId))
                .Column<string>(nameof(StripeSessionDataIndex.StripeSessionUrl))
                .Column<string>(nameof(StripeSessionDataIndex.StripeInvoiceId))
                .Column<string>(nameof(StripeSessionDataIndex.SerializedAdditionalData))
            );

        return 1;
    }
}
