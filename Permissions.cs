using OrchardCore.Security.Permissions;
using System.Collections.Generic;

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
