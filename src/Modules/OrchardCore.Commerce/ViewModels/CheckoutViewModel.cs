using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Commerce.ViewModels;

public class CheckoutViewModel
{
    public IShape NewOrderEditor { get; init; }
    public Amount SingleCurrencyTotal { get; init; }
    public string StripePublishableKey { get; init; }
}
