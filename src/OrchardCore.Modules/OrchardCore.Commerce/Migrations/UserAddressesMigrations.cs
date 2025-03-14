using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Abstractions.Constants.ContentTypes;

namespace OrchardCore.Commerce.Migrations;

public class UserAddressesMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public UserAddressesMigrations(IContentDefinitionManager contentDefinitionManager) =>
        _contentDefinitionManager = contentDefinitionManager;

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager
            .AlterPartDefinitionAsync<UserAddressesPart>(builder => builder
                .WithField(part => part.BillingAddress, field => field.WithDisplayName("Billing Address"))
                .WithField(part => part.ShippingAddress, field => field.WithDisplayName("Shipping Address"))
                .WithField(part => part.BillingAndShippingAddressesMatch, field => field
                    .WithDisplayName("Shipping Address and Billing Address are the same")));

        await _contentDefinitionManager
            .AlterTypeDefinitionAsync(UserAddresses, builder => builder
                .Stereotype("CustomUserSettings")
                .WithPart(nameof(UserAddressesPart)));

        return 1;
    }
}
