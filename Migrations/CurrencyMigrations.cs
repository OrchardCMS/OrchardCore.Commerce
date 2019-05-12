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
                .Creatable()
                .Listable()
                .WithPart("TitlePart")
                //.WithPart("Currency")           // Only needed to enable adding more fields to the type.
                .WithPart("CurrencyPart")
            );

            _contentDefinitionManager.AlterPartDefinition("CurrencyPart", builder => builder
                //.Attachable()
                .WithDescription("Currency part")
                //.WithField("Name", field => field
                //    .OfType("TextField")
                //    .WithSetting("DisplayName", "Name")
                //    .WithSetting("Position", "0")
                //)
                //.WithField("IsoCode", field => field
                //    .OfType("TextField")
                //    .WithSetting("DisplayName", "IsoCode")
                //    .WithSetting("Position", "1")
                //)
                //.WithField("Symbol", field => field
                //    .OfType("TextField")
                //    .WithSetting("DisplayName", "Symbol")
                //    .WithSetting("Position", "2")
                //)
                //.WithField("DecimalPlaces", field => field
                //    .OfType("NumericField")
                //    .WithSetting("DisplayName", "Decimal places")
                //    .WithSetting("Position", "3")
                //)
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
