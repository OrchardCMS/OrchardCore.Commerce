using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.Commerce.Indexes;
using OrchardCore.Lists.Models;

namespace OrchardCore.Commerce.Migrations
{
    /// <summary>
    /// Adds price book approach to pricing to the list of available parts.
    /// </summary>
    public class PriceBookMigrations : DataMigration
    {
        IContentDefinitionManager _contentDefinitionManager;

        public PriceBookMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            // Content Parts
            _contentDefinitionManager.AlterPartDefinition("PriceBookPart", builder => builder
                .Attachable(false)
                .WithDescription("Provides unique elements needed for a price book."));

            _contentDefinitionManager.AlterPartDefinition("PriceBookEntryPart", builder => builder
                .Attachable(false)
                .WithDescription("Provides unique elements needed for a price book entry."));

            _contentDefinitionManager.AlterPartDefinition("PriceBookProductPart", builder => builder
                .Attachable()
                .WithDescription("Provides related price book entries for a product."));
            _contentDefinitionManager.AlterPartDefinition("PriceBookRulePart", builder => builder
                .Attachable(false)
                .WithDescription("Provides additional options for price book rules."));

            // Content Types
            _contentDefinitionManager.AlterTypeDefinition("PriceBookEntry", builder => builder
                .DisplayedAs("Price Book Entry")
                .Draftable()
                .Versionable()
                .WithPart("PriceBookEntryPart")
                .WithPart("PricePart")
                );

            _contentDefinitionManager.AlterTypeDefinition("PriceBook", builder => builder
                .DisplayedAs("Price Book")
                .Draftable()
                .Versionable()
                .WithPart("TitlePart")
                .WithPart("PriceBookPart")
                .WithPart("PriceBook")
                .WithPart("ListPart", c => c
                    .WithSettings(new ListPartSettings { 
                        ContainedContentTypes = new string[] { "PriceBookEntry" } 
                    })
                )
            );

            
            // Indexes
            SchemaBuilder.CreateMapIndexTable(nameof(PriceBookEntryPartIndex), table => table
                .Column<string>("ContentItemId", c => c.WithLength(26))
                .Column<string>("PriceBookContentItemId", c => c.WithLength(26))
                .Column<string>("ProductContentItemId", c => c.WithLength(26))
                .Column<bool>("UseStandardPrice")
                .Column<decimal>("AmountValue", c => c.Nullable())
                .Column<string>("AmountCurrencyIsoCode", c => c.WithLength(3)));

            SchemaBuilder.AlterTable(nameof(PriceBookEntryPartIndex), table => table
                .CreateIndex("IDX_PriceBookEntryPartIndex_PriceBookContentItemId", "PriceBookContentItemId")
            );

            SchemaBuilder.AlterTable(nameof(PriceBookEntryPartIndex), table => table
                .CreateIndex("IDX_PriceBookEntryPartIndex_ProductContentItemId", "ProductContentItemId")
            );

            return 1;
        }
    }
}