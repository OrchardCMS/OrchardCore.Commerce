using OrchardCore.Commerce.Abstractions.Models;

namespace OrchardCore.Commerce.Models;

public record DeserializeResult(
    ShoppingCart ShoppingCart,
    bool HasChanged,
    bool IsEmpty);
