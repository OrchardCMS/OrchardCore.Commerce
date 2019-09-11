using OrchardCore.Security.Permissions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageCommerceSettings = new Permission("ManageCommerceSettings", "Manage Commerce Settings");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[]
            {
                ManageCommerceSettings,
            };
        }

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageCommerceSettings }
                },
            };
        }
    }
}
