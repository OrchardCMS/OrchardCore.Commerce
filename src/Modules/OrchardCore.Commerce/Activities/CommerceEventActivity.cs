using Lombiq.HelpfulLibraries.OrchardCore.Workflow;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Commerce.Activities;

public abstract class CommerceEventActivity : SimpleEventActivity
{
    public override LocalizedString Category => T["Commerce"];

    protected CommerceEventActivity(IStringLocalizer stringLocalizer)
        : base(stringLocalizer)
    {
    }
}
