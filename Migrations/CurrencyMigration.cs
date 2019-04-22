using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace OrchardCore.Commerce.Migrations
{
    public class CurrencyMigration : DataMigration
    {
        IContentDefinitionManager _contentDefinitionManager;

        public CurrencyMigration(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterTypeDefinition("Currency", builder => builder
                .Creatable()
                .WithPart("TitlePart")
                .WithPart("Currency")
            );

            _contentDefinitionManager.AlterPartDefinition("Currency", builder => builder
                .WithField("Name", field => field
                    .OfType("TextField")
                    .WithSetting("DisplayName", "Name")
                    .WithSetting("Position", "0")
                )
                .WithField("IsoCode", field => field
                    .OfType("TextField")
                    .WithSetting("DisplayName", "IsoCode")
                    .WithSetting("Position", "1")
                )
                .WithField("Symbol", field => field
                    .OfType("TextField")
                    .WithSetting("DisplayName", "Symbol")
                    .WithSetting("Position", "2")
                )
                .WithField("DecimalPlaces", field => field
                    .OfType("NumericField")
                    .WithSetting("DisplayName", "Decimal places")
                    .WithSetting("Position", "3")
                )
            );

            return 1;
        }
    }
}
