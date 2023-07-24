using Microsoft.AspNetCore.Authorization;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Security;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Handlers;

public class OrderPermissionsAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly Lazy<IAuthorizationService> _authorizationServiceLazy;

    public OrderPermissionsAuthorizationHandler(Lazy<IAuthorizationService> authorizationServiceLazy) =>
        _authorizationServiceLazy = authorizationServiceLazy;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.Resource is not IContent order || order.As<OrderPart>() is null)
        {
            return;
        }

        // Regular users should only see their own Orders, while users with the ManageOrders permission should be
        // able to see all Orders.
        if (!await _authorizationServiceLazy.Value.AuthorizeAsync(context.User, Permissions.ManageOrders) &&
            context.User.Identity.Name != order.ContentItem.Author)
        {
            context.Fail();
        }

        return;
    }
}
