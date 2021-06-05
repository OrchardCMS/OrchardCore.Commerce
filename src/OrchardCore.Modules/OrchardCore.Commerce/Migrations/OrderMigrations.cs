using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Settings;

namespace OrchardCore.Commerce.Migrations
{
    /// <summary>
    /// Adds the order part to the list of available parts and the address field to the list of available fields.
    /// </summary>
    public class OrderMigrations : DataMigration
    {
        IContentDefinitionManager _contentDefinitionManager;

        public OrderMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition("OrderPart", builder => builder
                .Attachable()
                .WithDescription("Makes a content item into an order."));

            _contentDefinitionManager.MigrateFieldSettings<AddressField, AddressPartFieldSettings>();

            return 1;
        }
    }
}