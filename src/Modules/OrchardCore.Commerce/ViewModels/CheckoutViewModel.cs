using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Views;
using System.Collections.Generic;

namespace OrchardCore.Commerce.ViewModels;

public class CheckoutViewModel : ShapeViewModel, ICheckoutViewModel
{
    public string ShoppingCartId { get; init; }
    public Amount SingleCurrencyTotal { get; init; }
    public Amount NetTotal { get; init; }
    public Amount GrossTotal { get; init; }
    public OrderPart OrderPart { get; init; }
    public string PaymentIntentClientSecret { get; init; }

    [BindNever]
    public IEnumerable<SelectListItem> Regions { get; set; }

    [BindNever]
    public IDictionary<string, IDictionary<string, string>> Provinces { get; } =
        new Dictionary<string, IDictionary<string, string>>();
    public string StripePublishableKey { get; init; }
    public string UserEmail { get; init; }
    public IEnumerable<IShape> CheckoutShapes { get; init; }

    public CheckoutViewModel() => Metadata.Type = "Checkout";
}
