using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.DisplayManagement.Views;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Payment.ViewModels;

public class PaymentViewModel : ShapeViewModel, IPaymentViewModel
{
    public Amount SingleCurrencyTotal { get; init; }

    public Amount NetTotal { get; init; }

    public OrderPart? OrderPart { get; init; }

    [BindNever]
    public IDictionary<string, object> PaymentProviderData { get; } = new Dictionary<string, object>();
}
