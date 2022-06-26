namespace OrchardCore.Commerce.ViewModels;
public class CardPaymentErrorViewModel
{
    public string RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
