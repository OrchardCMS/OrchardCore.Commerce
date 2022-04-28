using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Activities;

public class ProductAddedToCartEvent : EventActivity
{
    private readonly IStringLocalizer<ProductAddedToCartEvent> T;

    public override string Name => nameof(ProductAddedToCartEvent);

    public override LocalizedString DisplayText => T["Product added to cart"];

    public override LocalizedString Category => T["Commerce"];

    public ProductAddedToCartEvent(IStringLocalizer<ProductAddedToCartEvent> localizer) => T = localizer;

    public override IEnumerable<Outcome> GetPossibleOutcomes(
        WorkflowExecutionContext workflowContext,
        ActivityContext activityContext) =>
        Outcomes(new[] { T["Done"] });
}
