using Microsoft.Extensions.Localization;

namespace OrchardCore.Commerce.Activities;

public class CartDisplayingEvent : CommerceEventActivityBase
{
    public override LocalizedString DisplayText => T["Cart displaying"];

    public CartDisplayingEvent(IStringLocalizer<CartLoadedEvent> localizer)
        : base(localizer)
    {
    }
}
