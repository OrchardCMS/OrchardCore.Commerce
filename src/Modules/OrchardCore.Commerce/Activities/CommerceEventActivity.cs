using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Activities;

public abstract class CommerceEventActivity : EventActivity
{
    protected readonly IStringLocalizer T;

    public override string Name => GetType().Name;
    public abstract override LocalizedString DisplayText { get; }
    public override LocalizedString Category => T["Commerce"];

    protected CommerceEventActivity(IStringLocalizer stringLocalizer) =>
        T = stringLocalizer;

    public override IEnumerable<Outcome> GetPossibleOutcomes(
        WorkflowExecutionContext workflowContext,
        ActivityContext activityContext) =>
        new[] { new Outcome(T["Done"]) };

    public override ActivityExecutionResult Resume(
        WorkflowExecutionContext workflowContext,
        ActivityContext activityContext) =>
        Outcomes("Done");
}
