using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.DisplayManagement;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Payment.ViewModels;

public class CheckoutViewModel : PaymentViewModel, ICheckoutViewModel
{
    public string? ShoppingCartId { get; init; }

    public Amount GrossTotal { get; init; }

    [BindNever]
    public IEnumerable<SelectListItem> Regions => RegionData.CreateSelectListOptions();

    [BindNever]
    public IEnumerable<Region> RegionData { get; set; } = [];

    [BindNever]
    public IDictionary<string, IDictionary<string, string>> Provinces { get; } =
        new Dictionary<string, IDictionary<string, string>>();

    public string? UserEmail { get; init; }

    public bool IsInvalid { get; set; }

    public bool ShouldIgnoreAddress { get; set; }

    public IEnumerable<IShape> CheckoutShapes { get; init; } = [];

    public CheckoutViewModel(OrderPart orderPart, Amount singleCurrencyTotal, Amount netTotal)
        : base(orderPart, singleCurrencyTotal, netTotal) =>
        Metadata.Type = "Checkout";
}
