using OrchardCore.Commerce.AddressDataType;

namespace OrchardCore.Commerce.Models;

public record ProductEstimationContext(
    string ShoppingCartId,
    ShoppingCartItem ShoppingCartItem,
    Address ShippingAddress,
    Address BillingAddress);
