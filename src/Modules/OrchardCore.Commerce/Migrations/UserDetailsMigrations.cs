using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using static OrchardCore.Commerce.Constants.ContentTypes;

namespace OrchardCore.Commerce.Migrations;

public class UserDetailsMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public UserDetailsMigrations(IContentDefinitionManager contentDefinitionManager) =>
        _contentDefinitionManager = contentDefinitionManager;

    public int Create()
    {
        _contentDefinitionManager
            .AlterPartDefinition<UserDetailsPart>(builder => builder
                .WithField(part => part.PhoneNumber, field => field.WithDisplayName("Phone Number"))
                .WithField(part => part.VatNumber, field => field.WithDisplayName("VAT Number"))
                .WithField(part => part.IsCorporation, field => field
                    .WithDisplayName("User is a corporation")));

        _contentDefinitionManager
            .AlterTypeDefinition(UserDetails, builder => builder
                .Stereotype("CustomUserSettings")
                .WithPart(nameof(UserDetailsPart)));

        return 1;
    }
}
