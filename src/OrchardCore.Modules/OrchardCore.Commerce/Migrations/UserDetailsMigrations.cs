using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Abstractions.Constants.ContentTypes;

namespace OrchardCore.Commerce.Migrations;

public class UserDetailsMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public UserDetailsMigrations(IContentDefinitionManager contentDefinitionManager) =>
        _contentDefinitionManager = contentDefinitionManager;

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager
            .AlterPartDefinitionAsync<UserDetailsPart>(builder => builder
                .WithField(part => part.PhoneNumber, field => field.WithDisplayName("Phone Number"))
                .WithField(part => part.VatNumber, field => field.WithDisplayName("VAT Number"))
                .WithField(part => part.IsCorporation, field => field.WithDisplayName("User is a corporation")));

        await _contentDefinitionManager
            .AlterTypeDefinitionAsync(UserDetails, builder => builder
                .Stereotype("CustomUserSettings")
                .WithPart(nameof(UserDetailsPart)));

        return 1;
    }
}
