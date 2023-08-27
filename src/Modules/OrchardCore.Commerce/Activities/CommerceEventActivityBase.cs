using Lombiq.HelpfulLibraries.OrchardCore.Workflow;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Commerce.Activities;

public abstract class CommerceEventActivityBase : SimpleEventActivityBase
{
    public override LocalizedString Category => T["Commerce"];

    protected CommerceEventActivityBase(IStringLocalizer stringLocalizer)
        : base(stringLocalizer)
    {
    }
}
