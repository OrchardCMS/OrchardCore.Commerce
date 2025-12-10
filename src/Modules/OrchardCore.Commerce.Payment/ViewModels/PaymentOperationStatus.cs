using System.Text.Json.Serialization;

namespace OrchardCore.Commerce.Payment.ViewModels;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentOperationStatus
{
    Succeeded = 0,
    WaitingForRedirect = 1,
    NotFound = 2,
    Failed = 3,
    NotThingToDo = 4,
}
