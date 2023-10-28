using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.DisplayManagement.Views;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.ViewModels;

public class PaymentViewModel : ShapeViewModel, IPaymentViewModel
{
    public Amount SingleCurrencyTotal { get; init; }

    public Amount NetTotal { get; init; }

    public OrderPart? OrderPart { get; init; }

    [BindNever]
    public IDictionary<string, object> PaymentProviderData { get; } = new Dictionary<string, object>();

    public async Task WithProviderDataAsync(IEnumerable<IPaymentProvider> paymentProviders)
    {
        foreach (var provider in paymentProviders)
        {
            if (await provider.CreatePaymentProviderDataAsync(this) is { } data)
            {
                PaymentProviderData[provider.Name] = data;
            }
        }
    }
}
