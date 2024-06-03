using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Endpoints.ViewModels;
public class ProductListVM : ProductList
{
    public ProductListVM(ProductList list)
    {
        this.Products = list.Products;
        this.TotalItemCount = list.TotalItemCount;
    }
  
    // 摘要:
    //     Gets or sets the current page number or the default page number if none is specified.
    public int PageNum { get; set; }

    //
    // 摘要:
    //     Gets or sets the current page size or the site default size if none is specified.
    public int PageSize { get; set; }
}
