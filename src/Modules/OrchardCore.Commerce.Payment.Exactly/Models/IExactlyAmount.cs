using OrchardCore.Commerce.MoneyDataType;
using System.Globalization;

namespace OrchardCore.Commerce.Payment.Exactly.Models;

/// <summary>
/// Interface for types that represent an amount, and so may be initialized from an <see cref="OrchardCore.Commerce"/>.
/// </summary>
public interface IExactlyAmount
{
    /// <summary>
    /// Gets or sets the decimal amount represented as string. Point (".") must be used as decimal separator. Decimal
    /// separator must always be present for currencies with minor units. The maximum length is 37 characters: 18 digits
    /// before the decimal separator, the decimal separator and 18 digits after.
    /// </summary>
    public string Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency code, can be ISO 4217 alpha code or unofficial, e.g. XBT for Bitcoin.
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
