using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Payment.ViewModels;
using OrchardCore.DisplayManagement;
using System.Collections.Generic;

namespace OrchardCore.Commerce.ViewModels;

public class CheckoutViewModel : PaymentViewModel, ICheckoutViewModel
{
    public string ShoppingCartId { get; init; }
    public Amount GrossTotal { get; init; }

    [BindNever]
    public IEnumerable<SelectListItem> Regions { get; set; }

    [BindNever]
    public IDictionary<string, IDictionary<string, string>> Provinces { get; } =
        new Dictionary<string, IDictionary<string, string>>();
    public string UserEmail { get; init; }
    public IEnumerable<IShape> CheckoutShapes { get; init; }

    public CheckoutViewModel() => Metadata.Type = "Checkout";
}
