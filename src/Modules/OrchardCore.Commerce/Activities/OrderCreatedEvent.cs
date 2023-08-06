using Microsoft.Extensions.Localization;

namespace OrchardCore.Commerce.Activities;

public class OrderCreatedEvent : CommerceEventActivity
{
    public OrderCreatedEvent(IStringLocalizer<OrderCreatedEvent> localizer)
        : base(localizer)
    {
    }

    public override LocalizedString DisplayText => T["Order was created"];
}
