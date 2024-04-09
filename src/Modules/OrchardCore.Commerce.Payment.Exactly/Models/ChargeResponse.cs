using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;

namespace OrchardCore.Commerce.Payment.Exactly.Models;

public class ChargeResponse : IExactlyResponseData
{
    private static readonly Dictionary<string, ChargeResponseStatus> _statusMap = new()
    {
        ["action-required"] = ChargeResponseStatus.ActionRequired,
        ["processing"] = ChargeResponseStatus.Processing,
        ["processed"] = ChargeResponseStatus.Processed,
        ["failed"] = ChargeResponseStatus.Failed,
    };

    public string Type => "charge";
    public string Id { get; set; }
    public ChargeAttributes Attributes { get; set; }

    [SuppressMessage(
        "StyleCop.CSharp.NamingRules",
        "SA1313:Parameter names should begin with lower-case letter",
        Justification = "Necessary for localization extractor.")]
    public static IDictionary<string, LocalizedString> GetResultCodes(IStringLocalizer<ChargeResponse> T) =>
        new Dictionary<string, LocalizedString>
        {
            ["success"] = T["processing of the transaction was successfully completed"],
            ["transaction_failed"] = T["transaction failed, no specific details could be provided"],
            ["refund_not_allowed_for_transaction_in_progress"] = T["refund is not allowed for the payment in progress"],
            ["refundable_amount_exceeded"] = T["refund amount exceeds the amount of the original payment"],
            ["refunds_restricted_for_transaction"] = T["refunds not allowed for the original payment"],
            ["reversals_restricted_for_transaction"] = T["reversals not allowed for the original payment"],
            ["reversible_amount_exceeded"] = T["amount to reverse exceeds the amount of the original payment"],
            ["not_allowed_for_failed_transaction"] = T["the operation is not allowed for the failed transaction"],
            ["sub_recurrings_restricted_for_transaction"] = T["subsequent recurring payments are not allowed for the initial payment"],
            ["already_captured"] = T["payment was already captured"],
            ["capture_amount_exceeded"] = T["capture amount exceeds the amount of the original authorize"],
            ["authentication_failed"] = T["customer failed 3DS or any other authentication"],
            ["authentication_expired"] = T["customer didn't complete 3DS or any other authentication in expected time"],
            ["invalid_card_data"] = T["invalid card details were provided"],
            ["cancelled_by_customer"] = T["customer cancelled the transaction"],
            ["blocked_by_issuer"] = T["the transaction was blocked by the issuer"],
            ["declined_by_issuer"] = T["the transaction was declined by the issuer"],
            ["insufficient_funds"] = T["customer's account doesn't have enough funds for the payment"],
            ["insufficient_balance"] = T["merchant's account doesn't have enough funds for the transaction"],
            ["try_again_later"] = T["transaction declined, wait for some time before re-trying the transaction"],
            ["try_again"] = T["transaction declined, re-try the transaction"],
            ["transaction_declined"] = T["transaction declined, no additional details provided"],
            ["contact_support"] = T["transaction declined, please contact Support Team providing the transaction ID"],
            ["country_not_allowed"] = T["customer location or issuer country is not allowed"],
            ["count_limit_exceeded"] = T["allowed limit for total number of transactions was reached"],
            ["volume_limit_exceeded"] = T["allowed limit for total volume was reached"],
            ["amount_too_large"] = T["amount of the transaction is too large"],
            ["amount_too_small"] = T["amount of the transaction is too small"],
            ["invalid_transaction_attributes"] =
                T["invalid transaction attributes provided by customer or merchant, e.g. card details, customer details"],
            ["fraud_suspected"] = T["the transaction was declined due to suspected fraud"],
            ["risk_check_failed"] = T["the transaction was declined due to failed risk check"],
            ["volume_limit_per_source_exceeded"] = T["allowed limit was reached for total volume per source, e.g. card, wallet account, etc."],
            ["card_blocked"] = T["the card is blocked"],
            ["expire_timeout"] = T["exceeded default allowed time for transaction processing"],
            ["custom_expire_timeout"] = T["exceeded allowed time for transaction processing"],
            ["contact_issuer"] = T["the transaction was declined, the customer should contact issuer"],
        };

    public Payment.Models.Payment ToPayment(IMoneyService moneyService)
    {
        var type = Attributes.Processing.PaymentMethod.Type;
        var amount = Attributes.Processing.GetAmount(moneyService);
        return new(
            type,
            Id,
            $"Exactly transaction via {type} for {amount}",
            amount,
            Attributes.CreatedAt);
    }

    public enum ChargeResponseStatus
    {
        /// <summary>
        /// Additional data or action (e.g., redirect) is required to proceed with transactions; required action details
        /// are available in <see cref="ChargeAttributes.Actions"/>.
        /// </summary>
        ActionRequired,

        /// <summary>
        /// Transaction is being processed.
        /// </summary>
        Processing,

        /// <summary>
        /// Processing of the transaction successfully completed; additional result details might be available in <see
        /// cref="ChargeProcessing.ResultCode"/>.
        /// </summary>
        Processed,

        /// <summary>
        /// Processing of transaction failed, reason of failure should be available in <see
        /// cref="ChargeProcessing.ResultCode"/>.
        /// </summary>
        Failed,
    }

    public class ChargeProcessing : IExactlyAmount
    {
        public PaymentMethod PaymentMethod { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public DateTime ProcessedAt { get; set; }
        public string ResultCode { get; set; }
    }

    public class ChargeAttributes
    {
        public string ProjectId { get; set; }

        [JsonPropertyName("status")]
        private string StringStatus { get; set; }

        [JsonIgnore]
        public ChargeResponseStatus Status
        {
            get => _statusMap.GetMaybe(StringStatus);
            set => StringStatus = _statusMap.Single(pair => pair.Value == value).Key;
        }

        [JsonPropertyName("environmentMode")]
        private string StringEnvironmentMode { get; set; }

        [JsonIgnore]
        public bool IsLive
        {
            get => StringEnvironmentMode == "live";
            set => StringEnvironmentMode = value ? "live" : "sandbox";
        }

        public DateTime CreatedAt { get; set; }

        public ChargeProcessing Processing { get; set; }
        public IEnumerable<object> StatusHistory { get; set; }
        public object Meta { get; set; }
        public string ReferenceId { get; set; }
        public DateTime ExpireAt { get; set; }
        public IEnumerable<ChargeAction> Actions { get; set; }
    }
}
