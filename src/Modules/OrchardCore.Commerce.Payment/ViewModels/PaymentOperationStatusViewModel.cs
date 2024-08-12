using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Payment.ViewModels;
public class PaymentOperationStatusViewModel
{
    private string? _hideMessage;
    public string? HideMessage
    {
        get => string.IsNullOrEmpty(_hideMessage) ? ShowMessage?.Value : _hideMessage;
        set => _hideMessage = value;
    }

    public PaymentOperationStatus Status { get; set; }
    public LocalizedHtmlString? ShowMessage { get; set; }
    public string? Url { get; set; }
    public ContentItem? Content { get; set; }
}
