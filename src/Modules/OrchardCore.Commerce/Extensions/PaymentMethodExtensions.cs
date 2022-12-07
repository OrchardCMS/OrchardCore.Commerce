using Stripe;

namespace OrchardCore.Commerce.Extensions;

public static class PaymentExtensions
{
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
}
