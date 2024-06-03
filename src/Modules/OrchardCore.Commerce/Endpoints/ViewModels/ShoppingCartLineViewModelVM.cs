using OrchardCore.Commerce.Abstractions.ViewModels;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Endpoints.ViewModels;
public class ShoppingCartLineViewModelVM : ShoppingCartLineViewModel
{
    public ShoppingCartLineViewModelVM(ShoppingCartLineViewModel shoppingCartLineViewModel)
        : base(shoppingCartLineViewModel.Attributes)
    {
        this.Errors = string.Empty;
        this.Product = shoppingCartLineViewModel.Product;
        this.ProductSku = shoppingCartLineViewModel.ProductSku;
        this.LinePrice = shoppingCartLineViewModel.LinePrice;
        this.UnitPrice = shoppingCartLineViewModel.UnitPrice;
        this.Quantity = shoppingCartLineViewModel.Quantity;
        this.ProductImageUrl = shoppingCartLineViewModel.ProductImageUrl;
        this.ProductName = shoppingCartLineViewModel.ProductName;
        this.AdditionalData = shoppingCartLineViewModel.AdditionalData;
    }
    public new IDictionary<string, Newtonsoft.Json.Linq.JToken> AdditionalData { get; set; }
    public string Errors { get; set; }
}
