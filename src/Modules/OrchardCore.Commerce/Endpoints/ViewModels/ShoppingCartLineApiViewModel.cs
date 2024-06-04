using OrchardCore.Commerce.Abstractions.ViewModels;

namespace OrchardCore.Commerce.Endpoints.ViewModels;
public class ShoppingCartLineApiViewModel : ShoppingCartLineViewModel
{
    public ShoppingCartLineApiViewModel(ShoppingCartLineViewModel shoppingCartLineViewModel)
        : base(shoppingCartLineViewModel.Attributes)
    {
        Errors = string.Empty;
        Product = shoppingCartLineViewModel.Product;
        ProductSku = shoppingCartLineViewModel.ProductSku;
        LinePrice = shoppingCartLineViewModel.LinePrice;
        UnitPrice = shoppingCartLineViewModel.UnitPrice;
        Quantity = shoppingCartLineViewModel.Quantity;
        ProductImageUrl = shoppingCartLineViewModel.ProductImageUrl;
        ProductName = shoppingCartLineViewModel.ProductName;
        AdditionalData = shoppingCartLineViewModel.AdditionalData;
    }

    public string Errors { get; set; }
}
