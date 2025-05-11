using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.DisplayManagement.Views;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.ViewModels;

public class PaymentViewModel : ShapeViewModel, IPaymentViewModel
{
    public Amount SingleCurrencyTotal { get; }

    public Amount NetTotal { get; }

    public OrderPart OrderPart { get; }

    [BindNever]
    public IDictionary<string, object> PaymentProviderData { get; } = new Dictionary<string, object>();

    public PaymentViewModel(OrderPart orderPart, Amount singleCurrencyTotal, Amount netTotal)
    {
        OrderPart = orderPart;
        SingleCurrencyTotal = singleCurrencyTotal;
        NetTotal = netTotal;
    }

    public async Task WithProviderDataAsync(
        IEnumerable<IPaymentProvider> paymentProviders,
        bool isPaymentRequest = false,
        string? shoppingCartId = null)
    {
        foreach (var provider in paymentProviders)
        {
            if (await provider.CreatePaymentProviderDataAsync(this, isPaymentRequest, shoppingCartId) is { } data)
            {
                PaymentProviderData[provider.Name] = data;
            }
        }
    }
}
