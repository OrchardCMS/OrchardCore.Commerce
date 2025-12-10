using OrchardCore.Commerce.Abstractions.Constants;
using OrchardCore.Commerce.Abstractions.Fields;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OrchardCore.Commerce.Abstractions.Models;

public class OrderPart : ContentPart
{
    public TextField OrderId { get; set; } = new();
    public TextField Status { get; set; } = new();

    /// <summary>
    /// Gets the order's line items.
    /// </summary>
    public IList<OrderLineItem> LineItems { get; } = [];

    /// <summary>
    /// Gets additional costs that don't belong to an <see cref="OrderLineItem"/>, such as taxes and shipping.
    /// </summary>
    public IList<OrderAdditionalCost> AdditionalCosts { get; } = [];

    /// <summary>
    /// Gets the amounts charged for this order. Typically, a single credit card charge.
    /// </summary>
    public IList<Payment> Charges { get; } = [];

    public TextField Email { get; set; } = new();
    public TextField Phone { get; set; } = new();
    public TextField VatNumber { get; set; } = new();

    public AddressField BillingAddress { get; set; } = new();
    public AddressField ShippingAddress { get; set; } = new();
    public BooleanField BillingAndShippingAddressesMatch { get; set; } = new();
    public BooleanField IsCorporation { get; set; } = new();

    public IDictionary<string, JsonNode> AdditionalData { get; } = new Dictionary<string, JsonNode>();

    [JsonIgnore]
    public bool IsPending => string.IsNullOrWhiteSpace(Status?.Text) || Status.Text.EqualsOrdinalIgnoreCase(OrderStatusCodes.Pending);

    [JsonIgnore]
    public bool IsOrdered => Status?.Text?.EqualsOrdinalIgnoreCase(OrderStatusCodes.Ordered) == true;

    [JsonIgnore]
    public bool IsFailed => Status?.Text?.EqualsOrdinalIgnoreCase(OrderStatusCodes.PaymentFailed) == true;

    /// <summary>
    /// Sets the <see cref="Status"/> to <see cref="OrderStatusCodes.PaymentFailed"/>.
    /// </summary>
    public void FailPayment() => Status.Text = OrderStatusCodes.PaymentFailed;

    /// <summary>
    /// Sets the <see cref="Status"/> to <see cref="OrderStatusCodes.Ordered"/>.
    /// </summary>
    public void SucceedPayment() => Status.Text = OrderStatusCodes.Ordered;
}
