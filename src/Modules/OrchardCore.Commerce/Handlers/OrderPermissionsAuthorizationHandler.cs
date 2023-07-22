using Microsoft.AspNetCore.Authorization;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Handlers;

public class OrderPermissionsAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly Lazy<IAuthorizationService> _authorizationServiceLazy;

    public OrderPermissionsAuthorizationHandler(Lazy<IAuthorizationService> authorizationServiceLazy)
    {
        _authorizationServiceLazy = authorizationServiceLazy;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.Resource is not IContent order || order.As<OrderPart>() is not { } orderPart)
        {
            return;
        }

        context.Fail();
        return;
    }
}
