using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Commerce
{
    public class PriceBookAdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer<AdminMenu> T;

        public PriceBookAdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder.Add(T["Configuration"], configuration => configuration
                .Add(T["Commerce"], "1", commerce => commerce
                    .Add(T["Price Books"], "1", entry => entry
                        .Action("Index", "PriceBooksAdmin", new { area = "OrchardCore.Commerce" })
                        .Permission(Permissions.ManageCommerceSettings)
                        .LocalNav())
                    )
                );

            builder.Add(T["Configuration"], configuration => configuration
                .Add(T["Commerce"], "1", commerce => commerce
                    .Add(T["Price Book Rules"], "2", entry => entry
                        .Action("Index", "PriceBookRulesAdmin", new { area = "OrchardCore.Commerce" })
                        .Permission(Permissions.ManageCommerceSettings)
                        .LocalNav())
                    )
                );

            return Task.CompletedTask;
        }
    }
}