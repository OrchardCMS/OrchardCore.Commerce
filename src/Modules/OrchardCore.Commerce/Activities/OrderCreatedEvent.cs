using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Activities;

public class OrderCreatedEvent : EventActivity
{
    private readonly IStringLocalizer<OrderCreatedEvent> T;

    public OrderCreatedEvent(IStringLocalizer<OrderCreatedEvent> localizer) =>
        T = localizer;

    public override string Name => nameof(OrderCreatedEvent);

    public override LocalizedString DisplayText => T["Order was created"];

    public override LocalizedString Category => T["Commerce"];

    public override IEnumerable<Outcome> GetPossibleOutcomes(
        WorkflowExecutionContext workflowContext,
        ActivityContext activityContext) =>
        new[] { new Outcome(T["Done"]) };

    public override ActivityExecutionResult Resume(
        WorkflowExecutionContext workflowContext,
        ActivityContext activityContext) =>
        Outcomes("Done");
}
