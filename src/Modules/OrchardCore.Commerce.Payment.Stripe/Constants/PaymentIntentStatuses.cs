namespace OrchardCore.Commerce.Constants;

public static class PaymentIntentStatuses
{
    public const string RequiresPaymentMethod = "requires_payment_method";
    public const string RequiresConfirmation = "requires_confirmation";
    public const string RequiresAction = "requires_action";
    public const string Processing = "processing";
    public const string RequiresCapture = "requires_capture";
    public const string Canceled = "canceled";
    public const string Succeeded = "succeeded";
}
