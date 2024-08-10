using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Payment.ViewModels;
public class PaidStatusViewModel
{
    public PaidStatus Status { get; set; }
    public LocalizedHtmlString? ShowMessage { get; set; }
    public string HideMessage { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public ContentItem? Content { get; set; }
}
