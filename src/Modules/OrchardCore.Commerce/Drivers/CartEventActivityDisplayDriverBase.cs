using Lombiq.HelpfulLibraries.OrchardCore.Workflow;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Activities;

namespace OrchardCore.Commerce.Drivers;

public abstract class CartEventActivityDisplayDriverBase<T> : SimpleEventActivityDisplayDriverBase<T>
    where T : CommerceEventActivityBase
{
    protected readonly IHtmlLocalizer H;

    public override string IconClass => "fa-shopping-cart";
    public abstract override LocalizedHtmlString Description { get; }

    protected CartEventActivityDisplayDriverBase(IHtmlLocalizer htmlLocalizer) =>
        H = htmlLocalizer;
}

public class CartDisplayingEventDisplayDriver : CartEventActivityDisplayDriverBase<CartDisplayingEvent>
{
    public override LocalizedHtmlString Description =>
        H["Invoked after the shopping cart data is prepared, but before the shapes are rendered."];

    public CartDisplayingEventDisplayDriver(IHtmlLocalizer<CartDisplayingEventDisplayDriver> htmlLocalizer)
        : base(htmlLocalizer)
    {
    }
}

public class CartVerifyingItemEventDisplayDriver : CartEventActivityDisplayDriverBase<CartVerifyingItemEvent>
{
    public override LocalizedHtmlString Description =>
        H["Invoked before an item is added to the shopping cart to check whether it can be added based on inventory status."];

    public CartVerifyingItemEventDisplayDriver(IHtmlLocalizer<CartVerifyingItemEventDisplayDriver> htmlLocalizer)
        : base(htmlLocalizer)
    {
    }
}

public class CartLoadedEventDisplayDriver : CartEventActivityDisplayDriverBase<CartLoadedEvent>
{
    public override LocalizedHtmlString Description =>
        H["Invoked after the shopping cart content is loaded from the store and before it's displayed or used for calculation."];

    public CartLoadedEventDisplayDriver(IHtmlLocalizer<CartLoadedEventDisplayDriver> htmlLocalizer)
        : base(htmlLocalizer)
    {
    }
}

public class ProductAddedToCartEventDisplayDriver : CartEventActivityDisplayDriverBase<ProductAddedToCartEvent>
{
    public override LocalizedHtmlString Description => H["Executes when a product is added to the shopping cart."];

    public ProductAddedToCartEventDisplayDriver(IHtmlLocalizer<ProductAddedToCartEventDisplayDriver> htmlLocalizer)
        : base(htmlLocalizer)
    {
    }
}
