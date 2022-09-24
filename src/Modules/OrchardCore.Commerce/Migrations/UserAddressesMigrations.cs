using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using static OrchardCore.Commerce.Constants.ContentTypes;

namespace OrchardCore.Commerce.Migrations;

public class UserAddressesMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public UserAddressesMigrations(IContentDefinitionManager contentDefinitionManager) =>
        _contentDefinitionManager = contentDefinitionManager;

    public int Create()
    {
        _contentDefinitionManager
            .AlterPartDefinition<UserAddressesPart>(builder => builder
                .WithField(part => part.BillingAddress, field => field.WithDisplayName("Billing Address"))
                .WithField(part => part.ShippingAddress, field => field.WithDisplayName("Shipping Address"))
                .WithField(part => part.BillingAndShippingAddressesMatch, field => field
                    .WithDisplayName("Shipping Address and Billing Address are the same.")));

        _contentDefinitionManager
            .AlterTypeDefinition(UserAddresses, builder => builder
                .Stereotype("CustomUserSettings")
                .WithPart(nameof(UserAddressesPart)));

        return 1;
    }
}
