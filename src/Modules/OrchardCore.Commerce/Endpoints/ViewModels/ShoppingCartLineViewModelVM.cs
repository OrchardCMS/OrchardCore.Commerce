using OrchardCore.Commerce.Abstractions.ViewModels;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Endpoints.ViewModels;
public class ShoppingCartLineViewModelVM : ShoppingCartLineViewModel
{
    public ShoppingCartLineViewModelVM(ShoppingCartLineViewModel shoppingCartLineViewModel)
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

    public new IDictionary<string, Newtonsoft.Json.Linq.JToken> AdditionalData { get; set; }
    public string Errors { get; set; }
}
