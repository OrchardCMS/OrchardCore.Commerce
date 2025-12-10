using OrchardCore.Commerce.MoneyDataType;
using Stripe;

namespace OrchardCore.Commerce.Payment.Stripe.Extensions;

public static class PaymentExtensions
{
    public static void AddExpansions(this BaseOptions baseOptions) => baseOptions.AddExpand("payment_method");

    public static string GetFormattedPaymentType(this PaymentMethod paymentMethod) =>
        paymentMethod.Type switch
        {
            "acss_debit" => "Pre-authorized debit payments",
            "affirm" => "Affirm",
            "afterpay_clearpay" => "Afterpay / Clearpay",
            "alipay" => "Alipay",
            "au_becs_debit" => "BECS Direct Debit ",
            "bacs_debit" => "Bacs Direct Debit",
            "bancontact" => "Bancontact",
            "blik" => "BLIK",
            "boleto" => "Boleto",
            "card" => "Card",
            "card_present" => "Stripe Terminal",
            "customer_balance" => "Cash balance",
            "eps" => "EPS",
            "fpx" => "FPX",
            "giropay" => "giropay",
            "grabpay" => "GrabPay",
            "ideal" => "iDEAL",
            "interac_present" => "Interac",
            "klarna" => "Klarna",
            "konbini" => "Konbini",
            "link" => "Link",
            "oxxo" => "OXXO",
            "p24" => "Przelewy24",
            "paynow" => "PayNow",
            "pix" => "Pix",
            "promptpay" => "PromptPay",
            "sepa_debit" => "SEPA Direct Debit",
            "sofort" => "Sofort",
            "us_bank_account" => "ACH Direct Debit",
            "wechat_pay" => "WeChat Pay",
            _ => paymentMethod.Type,
        };

    public static Commerce.Abstractions.Models.Payment CreatePayment(this PaymentIntent paymentIntent, Amount amount) =>
        new(
            Kind: paymentIntent.PaymentMethod?.GetFormattedPaymentType() ?? "Unset",
            ChargeText: paymentIntent.Description,
            TransactionId: paymentIntent.Id,
            Amount: amount,
            CreatedUtc: paymentIntent.Created);
}
