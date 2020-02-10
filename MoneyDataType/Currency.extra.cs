using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Money.Abstractions;

namespace Money
{
    partial struct Currency
    {
        public static ICurrency GetByIsoCode(string isoCode)
        {
            return DefaultProvider.GetCurrency(isoCode);
        }

        public static bool IsKnownCurrency(string isoCode)
        {
            return DefaultProvider.IsKnownCurrency(isoCode);
        }

        public static ICurrency UnitedArabEmiratesDirham => DefaultProvider.GetCurrency("AED");
        public static ICurrency AfghanAfghani => DefaultProvider.GetCurrency("AFN");
        public static ICurrency AlbanianLek => DefaultProvider.GetCurrency("ALL");
        public static ICurrency ArmenianDram => DefaultProvider.GetCurrency("AMD");
        public static ICurrency ArgentinePeso => DefaultProvider.GetCurrency("ARS");
        public static ICurrency AustralianDollar => DefaultProvider.GetCurrency("AUD");
        public static ICurrency AzerbaijaniManat => DefaultProvider.GetCurrency("AZN");
        public static ICurrency BosniaHerzegovinaConvertibleMark => DefaultProvider.GetCurrency("BAM");
        public static ICurrency BangladeshiTaka => DefaultProvider.GetCurrency("BDT");
        public static ICurrency BulgarianLev => DefaultProvider.GetCurrency("BGN");
        public static ICurrency BahrainiDinar => DefaultProvider.GetCurrency("BHD");
        public static ICurrency BruneiDollar => DefaultProvider.GetCurrency("BND");
        public static ICurrency BolivianBoliviano => DefaultProvider.GetCurrency("BOB");
        public static ICurrency BrazilianReal => DefaultProvider.GetCurrency("BRL");
        public static ICurrency BitCoin => DefaultProvider.GetCurrency("BTC");
        public static ICurrency BhutaneseNgultrum => DefaultProvider.GetCurrency("BTN");
        public static ICurrency BotswananPula => DefaultProvider.GetCurrency("BWP");
        public static ICurrency BelarusianRuble => DefaultProvider.GetCurrency("BYN");
        public static ICurrency BelizeDollar => DefaultProvider.GetCurrency("BZD");
        public static ICurrency CanadianDollar => DefaultProvider.GetCurrency("CAD");
        public static ICurrency CongoleseFranc => DefaultProvider.GetCurrency("CDF");
        public static ICurrency SwissFranc => DefaultProvider.GetCurrency("CHF");
        public static ICurrency ChileanPeso => DefaultProvider.GetCurrency("CLP");
        public static ICurrency ChineseYuan => DefaultProvider.GetCurrency("CNY");
        public static ICurrency ColombianPeso => DefaultProvider.GetCurrency("COP");
        public static ICurrency CostaRicanColón => DefaultProvider.GetCurrency("CRC");
        public static ICurrency CubanPeso => DefaultProvider.GetCurrency("CUP");
        public static ICurrency CzechKoruna => DefaultProvider.GetCurrency("CZK");
        public static ICurrency DanishKrone => DefaultProvider.GetCurrency("DKK");
        public static ICurrency DominicanPeso => DefaultProvider.GetCurrency("DOP");
        public static ICurrency AlgerianDinar => DefaultProvider.GetCurrency("DZD");
        public static ICurrency EgyptianPound => DefaultProvider.GetCurrency("EGP");
        public static ICurrency EritreanNakfa => DefaultProvider.GetCurrency("ERN");
        public static ICurrency EthiopianBirr => DefaultProvider.GetCurrency("ETB");
        public static ICurrency Euro => DefaultProvider.GetCurrency("EUR");
        public static ICurrency BritishPound => DefaultProvider.GetCurrency("GBP");
        public static ICurrency GeorgianLari => DefaultProvider.GetCurrency("GEL");
        public static ICurrency GuatemalanQuetzal => DefaultProvider.GetCurrency("GTQ");
        public static ICurrency HongKongDollar => DefaultProvider.GetCurrency("HKD");
        public static ICurrency HonduranLempira => DefaultProvider.GetCurrency("HNL");
        public static ICurrency CroatianKuna => DefaultProvider.GetCurrency("HRK");
        public static ICurrency HaitianGourde => DefaultProvider.GetCurrency("HTG");
        public static ICurrency HungarianForint => DefaultProvider.GetCurrency("HUF");
        public static ICurrency IndonesianRupiah => DefaultProvider.GetCurrency("IDR");
        public static ICurrency IsraeliNewShekel => DefaultProvider.GetCurrency("ILS");
        public static ICurrency IndianRupee => DefaultProvider.GetCurrency("INR");
        public static ICurrency IraqiDinar => DefaultProvider.GetCurrency("IQD");
        public static ICurrency IranianRial => DefaultProvider.GetCurrency("IRR");
        public static ICurrency IcelandicKróna => DefaultProvider.GetCurrency("ISK");
        public static ICurrency JamaicanDollar => DefaultProvider.GetCurrency("JMD");
        public static ICurrency JordanianDinar => DefaultProvider.GetCurrency("JOD");
        public static ICurrency JapaneseYen => DefaultProvider.GetCurrency("JPY");
        public static ICurrency KenyanShilling => DefaultProvider.GetCurrency("KES");
        public static ICurrency KyrgystaniSom => DefaultProvider.GetCurrency("KGS");
        public static ICurrency CambodianRiel => DefaultProvider.GetCurrency("KHR");
        public static ICurrency SouthKoreanWon => DefaultProvider.GetCurrency("KRW");
        public static ICurrency KuwaitiDinar => DefaultProvider.GetCurrency("KWD");
        public static ICurrency KazakhstaniTenge => DefaultProvider.GetCurrency("KZT");
        public static ICurrency LaotianKip => DefaultProvider.GetCurrency("LAK");
        public static ICurrency LebanesePound => DefaultProvider.GetCurrency("LBP");
        public static ICurrency SriLankanRupee => DefaultProvider.GetCurrency("LKR");
        public static ICurrency LibyanDinar => DefaultProvider.GetCurrency("LYD");
        public static ICurrency MoroccanDirham => DefaultProvider.GetCurrency("MAD");
        public static ICurrency MoldovanLeu => DefaultProvider.GetCurrency("MDL");
        public static ICurrency MacedonianDenar => DefaultProvider.GetCurrency("MKD");
        public static ICurrency MyanmarKyat => DefaultProvider.GetCurrency("MMK");
        public static ICurrency MongolianTugrik => DefaultProvider.GetCurrency("MNT");
        public static ICurrency MacanesePataca => DefaultProvider.GetCurrency("MOP");
        public static ICurrency MaldivianRufiyaa => DefaultProvider.GetCurrency("MVR");
        public static ICurrency MexicanPeso => DefaultProvider.GetCurrency("MXN");
        public static ICurrency MalaysianRinggit => DefaultProvider.GetCurrency("MYR");
        public static ICurrency NigerianNaira => DefaultProvider.GetCurrency("NGN");
        public static ICurrency NicaraguanCórdoba => DefaultProvider.GetCurrency("NIO");
        public static ICurrency NorwegianKrone => DefaultProvider.GetCurrency("NOK");
        public static ICurrency NepaleseRupee => DefaultProvider.GetCurrency("NPR");
        public static ICurrency NewZealandDollar => DefaultProvider.GetCurrency("NZD");
        public static ICurrency OmaniRial => DefaultProvider.GetCurrency("OMR");
        public static ICurrency PanamanianBalboa => DefaultProvider.GetCurrency("PAB");
        public static ICurrency PeruvianSol => DefaultProvider.GetCurrency("PEN");
        public static ICurrency PhilippinePiso => DefaultProvider.GetCurrency("PHP");
        public static ICurrency PakistaniRupee => DefaultProvider.GetCurrency("PKR");
        public static ICurrency PolishZloty => DefaultProvider.GetCurrency("PLN");
        public static ICurrency ParaguayanGuarani => DefaultProvider.GetCurrency("PYG");
        public static ICurrency QatariRial => DefaultProvider.GetCurrency("QAR");
        public static ICurrency RomanianLeu => DefaultProvider.GetCurrency("RON");
        public static ICurrency SerbianDinar => DefaultProvider.GetCurrency("RSD");
        public static ICurrency RussianRuble => DefaultProvider.GetCurrency("RUB");
        public static ICurrency RwandanFranc => DefaultProvider.GetCurrency("RWF");
        public static ICurrency SaudiRiyal => DefaultProvider.GetCurrency("SAR");
        public static ICurrency SwedishKrona => DefaultProvider.GetCurrency("SEK");
        public static ICurrency SingaporeDollar => DefaultProvider.GetCurrency("SGD");
        public static ICurrency SomaliShilling => DefaultProvider.GetCurrency("SOS");
        public static ICurrency SyrianPound => DefaultProvider.GetCurrency("SYP");
        public static ICurrency ThaiBaht => DefaultProvider.GetCurrency("THB");
        public static ICurrency TajikistaniSomoni => DefaultProvider.GetCurrency("TJS");
        public static ICurrency TurkmenistaniManat => DefaultProvider.GetCurrency("TMT");
        public static ICurrency TunisianDinar => DefaultProvider.GetCurrency("TND");
        public static ICurrency TurkishLira => DefaultProvider.GetCurrency("TRY");
        public static ICurrency TrinidadAndTobagoDollar => DefaultProvider.GetCurrency("TTD");
        public static ICurrency NewTaiwanDollar => DefaultProvider.GetCurrency("TWD");
        public static ICurrency UkrainianHryvnia => DefaultProvider.GetCurrency("UAH");
        public static ICurrency USDollar => DefaultProvider.GetCurrency("USD");
        public static ICurrency UruguayanPeso => DefaultProvider.GetCurrency("UYU");
        public static ICurrency UzbekistaniSom => DefaultProvider.GetCurrency("UZS");
        public static ICurrency VenezuelanBolívar => DefaultProvider.GetCurrency("VES");
        public static ICurrency VietnameseDong => DefaultProvider.GetCurrency("VND");
        public static ICurrency CentralAfricanCFAFranc => DefaultProvider.GetCurrency("XAF");
        public static ICurrency EastCaribbeanDollar => DefaultProvider.GetCurrency("XCD");
        public static ICurrency SpecialDrawingRights => DefaultProvider.GetCurrency("XDR");
        public static ICurrency WestAfricanCFAFranc => DefaultProvider.GetCurrency("XOF");
        public static ICurrency YemeniRial => DefaultProvider.GetCurrency("YER");
        public static ICurrency SouthAfricanRand => DefaultProvider.GetCurrency("ZAR");

        private static readonly ICurrencyProvider DefaultProvider = new CurrencyProvider();

        public static ICurrency FromISOCode(string code, IEnumerable<ICurrencyProvider> providers = null)
        {
            var found = DefaultProvider.GetCurrency(code);
            return found ?? (providers ?? new List<ICurrencyProvider>())
                .SelectMany(p => p.Currencies)
                .FirstOrDefault(c => c.CurrencyIsoCode == code);
        }

        public static ICurrency FromNativeName(string name, IEnumerable<ICurrencyProvider> providers = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new System.ArgumentException("Must provide a name", nameof(name));
            return (providers ?? new List<ICurrencyProvider>()).SelectMany(p => p.Currencies).FirstOrDefault(c => c.NativeName == name);
        }

        public static ICurrency FromEnglishName(string name, IEnumerable<ICurrencyProvider> providers = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new System.ArgumentException("Must provide a name", nameof(name));
            return (providers ?? new List<ICurrencyProvider>()).SelectMany(p => p.Currencies).FirstOrDefault(c => c.EnglishName == name);
        }

        public static ICurrency FromRegion(RegionInfo region, IEnumerable<ICurrencyProvider> providers = null)
        {
            if (region is null)
                throw new System.ArgumentNullException(nameof(region));
            var found = DefaultProvider.GetCurrency(region.ISOCurrencySymbol);
            return (providers ?? new List<ICurrencyProvider>())
                .SelectMany(p => p.Currencies)
                .FirstOrDefault(c => c.CurrencyIsoCode == region.ISOCurrencySymbol);
        }

        public static ICurrency FromCulture(CultureInfo culture, IEnumerable<ICurrencyProvider> providers = null)
        {
            KnownCurrencyTable.EnsureCurrencyTable();
            var temp = new Currency(culture);
            return temp != null ? (providers ?? new List<ICurrencyProvider>()).SelectMany(p => p.Currencies).FirstOrDefault(c => c.CurrencyIsoCode == temp.CurrencyIsoCode) : null;
        }
    }
}
