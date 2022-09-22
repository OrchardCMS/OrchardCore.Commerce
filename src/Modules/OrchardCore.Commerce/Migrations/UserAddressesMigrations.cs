using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

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
                .WithField(part => part.ShippingAddress, part => part.WithDisplayName("Shipping Address"))
                .WithField(part => part.BillingAddress, part => part.WithDisplayName("Billing Address")));

        _contentDefinitionManager
            .AlterTypeDefinition(Constants.ContentTypes.UserAddresses, builder => builder
                .Stereotype("CustomUserSettings")
                .WithPart(nameof(UserAddressesPart)));

        return 1;
    }
}
