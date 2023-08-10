using Microsoft.Extensions.Localization;

namespace OrchardCore.Commerce.Activities;

public class ProductAddedToCartEvent : CommerceEventActivityBase
{
    public override LocalizedString DisplayText => T["Product added to cart"];

    public ProductAddedToCartEvent(IStringLocalizer<ProductAddedToCartEvent> localizer)
        : base(localizer)
    {
    }
}
