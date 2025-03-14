using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Payment.Endpoints.Api;

namespace OrchardCore.Commerce.Payment.Endpoints.Extensions;

public static class Endpoints
{
    public static IEndpointRouteBuilder AddPaymentApiEndpoints(this IEndpointRouteBuilder router)
    {
        router
            .AddFreeEndpoint()
            .AddCallbackEndpoint()
            .AddPaymentRequestEndpoint();

        return router;
    }
}
