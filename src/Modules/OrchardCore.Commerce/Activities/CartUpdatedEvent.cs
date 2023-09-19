using Microsoft.Extensions.Localization;

namespace OrchardCore.Commerce.Activities;

public class CartUpdatedEvent : CommerceEventActivityBase
{
    public override LocalizedString DisplayText => T["Cart updated"];

    public CartUpdatedEvent(IStringLocalizer<CartUpdatedEvent> localizer)
        : base(localizer)
    {
    }
}
