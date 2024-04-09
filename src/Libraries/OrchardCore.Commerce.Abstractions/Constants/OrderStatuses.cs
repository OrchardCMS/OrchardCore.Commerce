namespace OrchardCore.Commerce.Abstractions.Constants;

public static class OrderStatuses
{
    public const string Pending = nameof(Pending);
    public const string PaymentFailed = nameof(PaymentFailed);
    public const string Ordered = nameof(Ordered);
    public const string Arrived = nameof(Arrived);
    public const string Shipped = nameof(Shipped);

    public static class OrderStatusCodes
    {
        public const string Pending = "pending";
        public const string PaymentFailed = "payment-failed";
        public const string Ordered = "ordered";
        public const string Arrived = "arrived";
        public const string Shipped = "shipped";
    }
}
