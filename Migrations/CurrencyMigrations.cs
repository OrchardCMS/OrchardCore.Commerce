using OrchardCore.Commerce.Indexes;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace OrchardCore.Commerce.Migrations
{
    public class CurrencyMigrations : DataMigration
    {
        IContentDefinitionManager _contentDefinitionManager;

        public CurrencyMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterTypeDefinition("Currency", builder => builder
                .WithPart("CurrencyPart")
            );

            _contentDefinitionManager.AlterPartDefinition("CurrencyPart", builder => builder
                .WithDescription("Currency part")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(CurrencyPartIndex), table => table
                .Column<string>("IsoCode", c => c.WithLength(3))
                .Column<string>("ContentItemId", c => c.WithLength(26))
            );

            SchemaBuilder.AlterTable(nameof(CurrencyPartIndex), table => table
                .CreateIndex("IDX_CurrencyPartIndex_IsoCode", "IsoCode")
            );

            return 1;
        }
    }
}
