using Money.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Money;

public readonly partial struct Currency
{
    public static ICurrency GetByIsoCode(string isoCode) => _defaultProvider.GetCurrency(isoCode);

    public static bool IsKnownCurrency(string isoCode) => _defaultProvider.IsKnownCurrency(isoCode);

    public static ICurrency UnitedArabEmiratesDirham => _defaultProvider.GetCurrency("AED");
    public static ICurrency AfghanAfghani => _defaultProvider.GetCurrency("AFN");
    public static ICurrency AlbanianLek => _defaultProvider.GetCurrency("ALL");
    public static ICurrency ArmenianDram => _defaultProvider.GetCurrency("AMD");
    public static ICurrency ArgentinePeso => _defaultProvider.GetCurrency("ARS");
    public static ICurrency AustralianDollar => _defaultProvider.GetCurrency("AUD");
    public static ICurrency AzerbaijaniManat => _defaultProvider.GetCurrency("AZN");
    public static ICurrency BosniaHerzegovinaConvertibleMark => _defaultProvider.GetCurrency("BAM");
    public static ICurrency BangladeshiTaka => _defaultProvider.GetCurrency("BDT");
    public static ICurrency BulgarianLev => _defaultProvider.GetCurrency("BGN");
    public static ICurrency BahrainiDinar => _defaultProvider.GetCurrency("BHD");
    public static ICurrency BruneiDollar => _defaultProvider.GetCurrency("BND");
    public static ICurrency BolivianBoliviano => _defaultProvider.GetCurrency("BOB");
    public static ICurrency BrazilianReal => _defaultProvider.GetCurrency("BRL");
    public static ICurrency BitCoin => _defaultProvider.GetCurrency("BTC");
    public static ICurrency BhutaneseNgultrum => _defaultProvider.GetCurrency("BTN");
    public static ICurrency BotswananPula => _defaultProvider.GetCurrency("BWP");
    public static ICurrency BelarusianRuble => _defaultProvider.GetCurrency("BYN");
    public static ICurrency BelizeDollar => _defaultProvider.GetCurrency("BZD");
    public static ICurrency CanadianDollar => _defaultProvider.GetCurrency("CAD");
    public static ICurrency CongoleseFranc => _defaultProvider.GetCurrency("CDF");
    public static ICurrency SwissFranc => _defaultProvider.GetCurrency("CHF");
    public static ICurrency ChileanPeso => _defaultProvider.GetCurrency("CLP");
    public static ICurrency ChineseYuan => _defaultProvider.GetCurrency("CNY");
    public static ICurrency ColombianPeso => _defaultProvider.GetCurrency("COP");
    public static ICurrency CostaRicanColón => _defaultProvider.GetCurrency("CRC");
    public static ICurrency CubanPeso => _defaultProvider.GetCurrency("CUP");
    public static ICurrency CzechKoruna => _defaultProvider.GetCurrency("CZK");
    public static ICurrency DanishKrone => _defaultProvider.GetCurrency("DKK");
    public static ICurrency DominicanPeso => _defaultProvider.GetCurrency("DOP");
    public static ICurrency AlgerianDinar => _defaultProvider.GetCurrency("DZD");
    public static ICurrency EgyptianPound => _defaultProvider.GetCurrency("EGP");
    public static ICurrency EritreanNakfa => _defaultProvider.GetCurrency("ERN");
    public static ICurrency EthiopianBirr => _defaultProvider.GetCurrency("ETB");
    public static ICurrency Euro => _defaultProvider.GetCurrency("EUR");
    public static ICurrency BritishPound => _defaultProvider.GetCurrency("GBP");
    public static ICurrency GeorgianLari => _defaultProvider.GetCurrency("GEL");
    public static ICurrency GuatemalanQuetzal => _defaultProvider.GetCurrency("GTQ");
    public static ICurrency HongKongDollar => _defaultProvider.GetCurrency("HKD");
    public static ICurrency HonduranLempira => _defaultProvider.GetCurrency("HNL");
    public static ICurrency CroatianKuna => _defaultProvider.GetCurrency("HRK");
    public static ICurrency HaitianGourde => _defaultProvider.GetCurrency("HTG");
    public static ICurrency HungarianForint => _defaultProvider.GetCurrency("HUF");
    public static ICurrency IndonesianRupiah => _defaultProvider.GetCurrency("IDR");
    public static ICurrency IsraeliNewShekel => _defaultProvider.GetCurrency("ILS");
    public static ICurrency IndianRupee => _defaultProvider.GetCurrency("INR");
    public static ICurrency IraqiDinar => _defaultProvider.GetCurrency("IQD");
    public static ICurrency IranianRial => _defaultProvider.GetCurrency("IRR");
    public static ICurrency IcelandicKróna => _defaultProvider.GetCurrency("ISK");
    public static ICurrency JamaicanDollar => _defaultProvider.GetCurrency("JMD");
    public static ICurrency JordanianDinar => _defaultProvider.GetCurrency("JOD");
    public static ICurrency JapaneseYen => _defaultProvider.GetCurrency("JPY");
    public static ICurrency KenyanShilling => _defaultProvider.GetCurrency("KES");
    public static ICurrency KyrgystaniSom => _defaultProvider.GetCurrency("KGS");
    public static ICurrency CambodianRiel => _defaultProvider.GetCurrency("KHR");
    public static ICurrency SouthKoreanWon => _defaultProvider.GetCurrency("KRW");
    public static ICurrency KuwaitiDinar => _defaultProvider.GetCurrency("KWD");
    public static ICurrency KazakhstaniTenge => _defaultProvider.GetCurrency("KZT");
    public static ICurrency LaotianKip => _defaultProvider.GetCurrency("LAK");
    public static ICurrency LebanesePound => _defaultProvider.GetCurrency("LBP");
    public static ICurrency SriLankanRupee => _defaultProvider.GetCurrency("LKR");
    public static ICurrency LibyanDinar => _defaultProvider.GetCurrency("LYD");
    public static ICurrency MoroccanDirham => _defaultProvider.GetCurrency("MAD");
    public static ICurrency MoldovanLeu => _defaultProvider.GetCurrency("MDL");
    public static ICurrency MacedonianDenar => _defaultProvider.GetCurrency("MKD");
    public static ICurrency MyanmarKyat => _defaultProvider.GetCurrency("MMK");
    public static ICurrency MongolianTugrik => _defaultProvider.GetCurrency("MNT");
    public static ICurrency MacanesePataca => _defaultProvider.GetCurrency("MOP");
    public static ICurrency MaldivianRufiyaa => _defaultProvider.GetCurrency("MVR");
    public static ICurrency MexicanPeso => _defaultProvider.GetCurrency("MXN");
    public static ICurrency MalaysianRinggit => _defaultProvider.GetCurrency("MYR");
    public static ICurrency NigerianNaira => _defaultProvider.GetCurrency("NGN");
    public static ICurrency NicaraguanCórdoba => _defaultProvider.GetCurrency("NIO");
    public static ICurrency NorwegianKrone => _defaultProvider.GetCurrency("NOK");
    public static ICurrency NepaleseRupee => _defaultProvider.GetCurrency("NPR");
    public static ICurrency NewZealandDollar => _defaultProvider.GetCurrency("NZD");
    public static ICurrency OmaniRial => _defaultProvider.GetCurrency("OMR");
    public static ICurrency PanamanianBalboa => _defaultProvider.GetCurrency("PAB");
    public static ICurrency PeruvianSol => _defaultProvider.GetCurrency("PEN");
    public static ICurrency PhilippinePiso => _defaultProvider.GetCurrency("PHP");
    public static ICurrency PakistaniRupee => _defaultProvider.GetCurrency("PKR");
    public static ICurrency PolishZloty => _defaultProvider.GetCurrency("PLN");
    public static ICurrency ParaguayanGuarani => _defaultProvider.GetCurrency("PYG");
    public static ICurrency QatariRial => _defaultProvider.GetCurrency("QAR");
    public static ICurrency RomanianLeu => _defaultProvider.GetCurrency("RON");
    public static ICurrency SerbianDinar => _defaultProvider.GetCurrency("RSD");
    public static ICurrency RussianRuble => _defaultProvider.GetCurrency("RUB");
    public static ICurrency RwandanFranc => _defaultProvider.GetCurrency("RWF");
    public static ICurrency SaudiRiyal => _defaultProvider.GetCurrency("SAR");
    public static ICurrency SwedishKrona => _defaultProvider.GetCurrency("SEK");
    public static ICurrency SingaporeDollar => _defaultProvider.GetCurrency("SGD");
    public static ICurrency SomaliShilling => _defaultProvider.GetCurrency("SOS");
    public static ICurrency SyrianPound => _defaultProvider.GetCurrency("SYP");
    public static ICurrency ThaiBaht => _defaultProvider.GetCurrency("THB");
    public static ICurrency TajikistaniSomoni => _defaultProvider.GetCurrency("TJS");
    public static ICurrency TurkmenistaniManat => _defaultProvider.GetCurrency("TMT");
    public static ICurrency TunisianDinar => _defaultProvider.GetCurrency("TND");
    public static ICurrency TurkishLira => _defaultProvider.GetCurrency("TRY");
    public static ICurrency TrinidadAndTobagoDollar => _defaultProvider.GetCurrency("TTD");
    public static ICurrency NewTaiwanDollar => _defaultProvider.GetCurrency("TWD");
    public static ICurrency UkrainianHryvnia => _defaultProvider.GetCurrency("UAH");
    public static ICurrency UsDollar => _defaultProvider.GetCurrency("USD");
    public static ICurrency UruguayanPeso => _defaultProvider.GetCurrency("UYU");
    public static ICurrency UzbekistaniSom => _defaultProvider.GetCurrency("UZS");
    public static ICurrency VenezuelanBolívar => _defaultProvider.GetCurrency("VES");
    public static ICurrency VietnameseDong => _defaultProvider.GetCurrency("VND");
    public static ICurrency CentralAfricanCfaFranc => _defaultProvider.GetCurrency("XAF");
    public static ICurrency EastCaribbeanDollar => _defaultProvider.GetCurrency("XCD");
    public static ICurrency SpecialDrawingRights => _defaultProvider.GetCurrency("XDR");
    public static ICurrency WestAfricanCfaFranc => _defaultProvider.GetCurrency("XOF");
    public static ICurrency YemeniRial => _defaultProvider.GetCurrency("YER");
    public static ICurrency SouthAfricanRand => _defaultProvider.GetCurrency("ZAR");

    private static readonly ICurrencyProvider _defaultProvider = new CurrencyProvider();

    public static ICurrency FromIsoCode(string code, IEnumerable<ICurrencyProvider> providers = null) =>
        _defaultProvider.GetCurrency(code) ??
        providers?.GetFirstCurrency(currency => currency.CurrencyIsoCode == code);

    public static ICurrency FromNativeName(string name, IEnumerable<ICurrencyProvider> providers = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Must provide a name.", nameof(name));
        }

        return providers?.GetFirstCurrency(currency => currency.NativeName == name);
    }

    public static ICurrency FromEnglishName(string name, IEnumerable<ICurrencyProvider> providers = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Must provide a name.", nameof(name));
        }

        return providers?.GetFirstCurrency(currency => currency.EnglishName == name);
    }

    public static ICurrency FromRegion(RegionInfo region, IEnumerable<ICurrencyProvider> providers = null)
    {
        ArgumentNullException.ThrowIfNull(region);

        return _defaultProvider.GetCurrency(region.ISOCurrencySymbol) ??
               FromIsoCode(region.ISOCurrencySymbol, providers);
    }

    public static ICurrency FromCulture(CultureInfo culture, IEnumerable<ICurrencyProvider> providers = null)
    {
        KnownCurrencyTable.EnsureCurrencyTable();
        var temp = new Currency(culture);
        return providers?.GetFirstCurrency(currency => currency.CurrencyIsoCode == temp.CurrencyIsoCode);
    }
}
