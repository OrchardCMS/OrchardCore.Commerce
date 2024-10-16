namespace OrchardCore.Commerce.Payment.Endpoints.Models;

public class AddCallbackViewModel
{
    public string PaymentProviderName { get; set; }
    public string? OrderId { get; set; }
    public string? ShoppingCartId { get; set; }
}
