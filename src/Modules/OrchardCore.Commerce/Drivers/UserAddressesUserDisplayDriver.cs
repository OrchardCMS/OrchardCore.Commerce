using Microsoft.AspNetCore.Html;
using OrchardCore.Commerce.Constants;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.ResourceManagement;
using OrchardCore.Users.Models;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers;

public class UserAddressesUserDisplayDriver : DisplayDriver<User>
{
    private readonly IResourceManager _resourceManager;

    public UserAddressesUserDisplayDriver(IResourceManager resourceManager) =>
        _resourceManager = resourceManager;

    public override Task<IDisplayResult> EditAsync(User model, BuildEditorContext context)
    {
        _resourceManager.RegisterResource("script", ResourceNames.ToggleSecondAddress).AtFoot();

        _resourceManager.RegisterFootScript(new HtmlString(
            "<script>" +
            "Array.from(document.querySelectorAll('.address_billing-address input, .address_shipping-address input'))" +
            ".forEach(element => element.required = false);" +
            "</script>"));

        return Task.FromResult<IDisplayResult>(null);
    }
}
