using Microsoft.Extensions.Localization;

namespace OrchardCore.Commerce.Activities;

public class CartLoadedEvent : CommerceEventActivityBase
{
    public override LocalizedString DisplayText => T["Cart loaded"];

    public CartLoadedEvent(IStringLocalizer<CartLoadedEvent> localizer)
        : base(localizer)
    {
    }
}
