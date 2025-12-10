namespace OrchardCore.Commerce.Payment.Stripe.Endpoints.Models;

public class ConfirmParametersViewModel
{
    public string ReturnUrl { get; set; }
    public string OrderId { get; set; }
}
