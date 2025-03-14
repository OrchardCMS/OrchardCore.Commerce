using Lombiq.HelpfulLibraries.OrchardCore.Workflow;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Activities;

namespace OrchardCore.Commerce.Drivers;

public class OrderCreatedEventDisplayDriver : SimpleEventActivityDisplayDriverBase<OrderCreatedEvent>
{
    private readonly IHtmlLocalizer<OrderCreatedEventDisplayDriver> H;
    public override string IconClass => "fa-truck";
    public override LocalizedHtmlString Description => H["Executes when an order is created on the frontend."];

    public OrderCreatedEventDisplayDriver(IHtmlLocalizer<OrderCreatedEventDisplayDriver> htmlLocalizer) =>
        H = htmlLocalizer;
}
