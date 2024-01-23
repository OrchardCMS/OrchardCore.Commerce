using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace OrchardCore.Commerce.Payment.Exactly.Models;

public class ChargeResponse : IExactlyResponseData
{
    private static Dictionary<string, ChargeResponseStatus> StatusMap = new()
    {
        ["action-required"] = ChargeResponseStatus.ActionRequired,
        ["processing"] = ChargeResponseStatus.Processing,
        ["processed"] = ChargeResponseStatus.Processed,
        ["failed"] = ChargeResponseStatus.Failed,
    };

    public string Type => "charge";
    public string Id { get; set; }
    public ChargeAttributes Attributes { get; set; }

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

    public class ChargeAction
    {
        public string Action { get; set; }
        public object RequiredAttributes { get; set; }
    }

    public class ChargeAttributes
    {
        public string ProjectId { get; set; }

        [JsonPropertyName("status")]
        private string StringStatus { get; set; }

        [JsonIgnore]
        public ChargeResponseStatus Status
        {
            get => StatusMap.GetMaybe(StringStatus);
            set => StringStatus = StatusMap.Single(pair => pair.Value == value).Key;
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
        public IList<object> StatusHistory { get; set; }
        public object Meta { get; set; }
        public string ReferenceId { get; set; }
        public DateTime ExpireAt { get; set; }
        public IList<ChargeAction> Actions { get; set; }
    }
}
