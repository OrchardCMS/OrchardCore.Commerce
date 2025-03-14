using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Endpoints.Api;

namespace OrchardCore.Commerce.Endpoints.Extensions;

public static class Endpoints
{
    public static IEndpointRouteBuilder AddShoppingCartApiEndpoints(this IEndpointRouteBuilder router)
    {
        router
            .AddNewOrderEndpoint()
            .AddUpdateEndpoint()
            .AddRemoveLineEndpoint()
            .AddGetCartEndpoint()
            .AddAddItemEndpoint();

        return router;
    }
}
