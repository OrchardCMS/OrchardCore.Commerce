using System.Text.Json.Serialization;

namespace OrchardCore.Commerce.Payment.ViewModels;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentStatus
{
    Succeeded = 0,
    WaitingPayment = 1,
    WaitingStripe = 2,
    NotFound = 3,
    Failed = 4,
    NotThingToDo = 5,
}
