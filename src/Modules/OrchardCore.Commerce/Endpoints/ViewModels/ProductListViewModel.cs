using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Endpoints.ViewModels;
public class ProductListViewModel : ProductList
{
    public ProductListViewModel(ProductList list)
    {
        Products = list.Products;
        TotalItemCount = list.TotalItemCount;
    }

    public int PageNum { get; set; }
    public int PageSize { get; set; }
}
