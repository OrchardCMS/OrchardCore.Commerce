using OrchardCore.Commerce.MoneyDataType;
using System.Globalization;

namespace OrchardCore.Commerce.Payment.Exactly.Models;

/// <summary>
/// Interface for types that represent an amount, and so may be initialized from an <see cref="OrchardCore.Commerce"/>.
/// </summary>
public interface IExactlyAmount
{
    /// <summary>
    /// Gets the decimal amount represented as string. Point (".") must be used as decimal separator. Decimal separator
    /// must always be present for currencies with minor units. Max len is 37 chars: 18 digits before decimal separator,
    /// decimal separator, 18 digits after
    /// </summary>
    public string Amount { get; set; }

    /// <summary>
    /// Gets the currency code, can be ISO 4217 alpha code or unofficial, e.g. XBT for Bitcoin.
    /// </summary>
    public string Currency { get; set; }
}

public static class ExactlyAmountExtensions
{
    public static void SetAmount(this IExactlyAmount target, Amount source)
    {
        target.Amount = source.Value.ToString("0.00", CultureInfo.InvariantCulture);
        target.Currency = source.Currency.CurrencyIsoCode;
    }

    public static Amount GetAmount(this IExactlyAmount source) =>
        new(
            decimal.Parse(source.Amount, CultureInfo.InvariantCulture),
            Currency.FromIsoCurrencyCode(source.Currency));
}
