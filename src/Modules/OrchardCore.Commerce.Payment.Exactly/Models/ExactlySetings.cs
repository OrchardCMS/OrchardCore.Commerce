using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Commerce.Payment.Exactly.Models;

public class ExactlySettings
{
    [Required]
    public string BaseAddress { get; set; }

    [Required]
    [PasswordPropertyText(password: true)]
    public string ApiKey { get; set; }
}
