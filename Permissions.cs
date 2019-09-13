using OrchardCore.Security.Permissions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageCommerceSettings = new Permission("ManageCommerceSettings", "Manage Commerce Settings");

        public IEnumerable<Permission> GetPermissions()
            => new[] { ManageCommerceSettings };

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
            => Task.FromResult(GetPermissions());
        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
            => new[] { new PermissionStereotype {
                Name = "Administrator",
                Permissions = new[] { ManageCommerceSettings }
            } };
    }
}
