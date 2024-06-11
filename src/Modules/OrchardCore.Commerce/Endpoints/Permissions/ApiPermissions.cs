using OrchardCore.Security.Permissions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Endpoints.Permissions;

public class ApiPermissions : IPermissionProvider
{
    public static readonly Permission CommerceApi = new("CommerceApi", "Manage Commerce Items Api");

    public Task<IEnumerable<Permission>> GetPermissionsAsync() => Task.FromResult(new[]
       {
                CommerceApi,
       }
       .AsEnumerable());

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() => new[]
        {
                new PermissionStereotype
                {
                    Name = "Administrator",
                },
                new PermissionStereotype
                {
                    Name = "Editor",
                },
                new PermissionStereotype
                {
                    Name = "Moderator",
                },
                new PermissionStereotype
                {
                    Name = "Author",
                },
                new PermissionStereotype
                {
                    Name = "Contributor",
                },
                new PermissionStereotype
                {
                    Name = "Authenticated",
                },
                new PermissionStereotype
                {
                    Name = "Anonymous",
                    Permissions = new[] { CommerceApi },
                },
        };
}
