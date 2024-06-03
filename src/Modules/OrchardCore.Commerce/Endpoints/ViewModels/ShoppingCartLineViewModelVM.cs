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

#pragma warning disable CA2227 // 集合属性应为只读
    public new IDictionary<string, Newtonsoft.Json.Linq.JToken> AdditionalData { get; set; }
#pragma warning restore CA2227 // 集合属性应为只读
    public string Errors { get; set; }
}
