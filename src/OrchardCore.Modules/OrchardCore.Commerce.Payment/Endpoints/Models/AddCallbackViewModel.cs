using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Commerce.Payment.Endpoints.Models;

public class AddCallbackViewModel
{
    [Required]
    public string? PaymentProviderName { get; set; }
    public string? OrderId { get; set; }
    public string? ShoppingCartId { get; set; }
}
