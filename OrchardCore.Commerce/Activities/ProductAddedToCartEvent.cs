using System;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Commerce.Activities;

public class ProductAddedToCartEvent : EventActivity
{
    private readonly IStringLocalizer<ProductAddedToCartEvent> _s;

    public ProductAddedToCartEvent(IStringLocalizer<ProductAddedToCartEvent> localizer) => _s = localizer;

    public override string Name => nameof(ProductAddedToCartEvent);

    public override LocalizedString DisplayText => _s["Product addded to cart"];

    public override LocalizedString Category => _s["Commerce"];

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcomes(_s["Done"]);
}