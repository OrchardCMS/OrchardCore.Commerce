using Microsoft.Extensions.Localization;

namespace OrchardCore.Commerce.Activities;

public class CartVerifyingItemEvent : CommerceEventActivityBase
{
    public override LocalizedString DisplayText => T["Verifying cart item"];

    public CartVerifyingItemEvent(IStringLocalizer<CartLoadedEvent> localizer)
        : base(localizer)
    {
    }
}
