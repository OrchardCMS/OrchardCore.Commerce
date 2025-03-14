namespace OrchardCore.Commerce.Payment.Exactly.Models;

public class ExactlyError
{
    public string Code { get; set; }
    public string Title { get; set; }
    public string Details { get; set; }
    public object Meta { get; set; }
}
