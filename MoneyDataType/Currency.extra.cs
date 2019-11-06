using OrchardCore.Commerce.Abstractions;
using System.Globalization;
using System.Linq;

namespace OrchardCore.Commerce.Money
{
    partial struct Currency
    {
        public static ICurrency UnitedArabEmiratesDirham => KnownCurrencyTable.FromIsoCode("AED");
        public static ICurrency AfghanAfghani => KnownCurrencyTable.FromIsoCode("AFN");
        public static ICurrency AlbanianLek => KnownCurrencyTable.FromIsoCode("ALL");
        public static ICurrency ArmenianDram => KnownCurrencyTable.FromIsoCode("AMD");
        public static ICurrency ArgentinePeso => KnownCurrencyTable.FromIsoCode("ARS");
        public static ICurrency AustralianDollar => KnownCurrencyTable.FromIsoCode("AUD");
        public static ICurrency AzerbaijaniManat => KnownCurrencyTable.FromIsoCode("AZN");
        public static ICurrency BosniaHerzegovinaConvertibleMark => KnownCurrencyTable.FromIsoCode("BAM");
        public static ICurrency BangladeshiTaka => KnownCurrencyTable.FromIsoCode("BDT");
        public static ICurrency BulgarianLev => KnownCurrencyTable.FromIsoCode("BGN");
        public static ICurrency BahrainiDinar => KnownCurrencyTable.FromIsoCode("BHD");
        public static ICurrency BruneiDollar => KnownCurrencyTable.FromIsoCode("BND");
        public static ICurrency BolivianBoliviano => KnownCurrencyTable.FromIsoCode("BOB");
        public static ICurrency BrazilianReal => KnownCurrencyTable.FromIsoCode("BRL");
        public static ICurrency BitCoin => KnownCurrencyTable.FromIsoCode("BTC");
        public static ICurrency BhutaneseNgultrum => KnownCurrencyTable.FromIsoCode("BTN");
        public static ICurrency BotswananPula => KnownCurrencyTable.FromIsoCode("BWP");
        public static ICurrency BelarusianRuble => KnownCurrencyTable.FromIsoCode("BYN");
        public static ICurrency BelizeDollar => KnownCurrencyTable.FromIsoCode("BZD");
        public static ICurrency CanadianDollar => KnownCurrencyTable.FromIsoCode("CAD");
        public static ICurrency CongoleseFranc => KnownCurrencyTable.FromIsoCode("CDF");
        public static ICurrency SwissFranc => KnownCurrencyTable.FromIsoCode("CHF");
        public static ICurrency ChileanPeso => KnownCurrencyTable.FromIsoCode("CLP");
        public static ICurrency ChineseYuan => KnownCurrencyTable.FromIsoCode("CNY");
        public static ICurrency ColombianPeso => KnownCurrencyTable.FromIsoCode("COP");
        public static ICurrency CostaRicanColón => KnownCurrencyTable.FromIsoCode("CRC");
        public static ICurrency CubanPeso => KnownCurrencyTable.FromIsoCode("CUP");
        public static ICurrency CzechKoruna => KnownCurrencyTable.FromIsoCode("CZK");
        public static ICurrency DanishKrone => KnownCurrencyTable.FromIsoCode("DKK");
        public static ICurrency DominicanPeso => KnownCurrencyTable.FromIsoCode("DOP");
        public static ICurrency AlgerianDinar => KnownCurrencyTable.FromIsoCode("DZD");
        public static ICurrency EgyptianPound => KnownCurrencyTable.FromIsoCode("EGP");
        public static ICurrency EritreanNakfa => KnownCurrencyTable.FromIsoCode("ERN");
        public static ICurrency EthiopianBirr => KnownCurrencyTable.FromIsoCode("ETB");
        public static ICurrency Euro => KnownCurrencyTable.FromIsoCode("EUR");
        public static ICurrency BritishPound => KnownCurrencyTable.FromIsoCode("GBP");
        public static ICurrency GeorgianLari => KnownCurrencyTable.FromIsoCode("GEL");
        public static ICurrency GuatemalanQuetzal => KnownCurrencyTable.FromIsoCode("GTQ");
        public static ICurrency HongKongDollar => KnownCurrencyTable.FromIsoCode("HKD");
        public static ICurrency HonduranLempira => KnownCurrencyTable.FromIsoCode("HNL");
        public static ICurrency CroatianKuna => KnownCurrencyTable.FromIsoCode("HRK");
        public static ICurrency HaitianGourde => KnownCurrencyTable.FromIsoCode("HTG");
        public static ICurrency HungarianForint => KnownCurrencyTable.FromIsoCode("HUF");
        public static ICurrency IndonesianRupiah => KnownCurrencyTable.FromIsoCode("IDR");
        public static ICurrency IsraeliNewShekel => KnownCurrencyTable.FromIsoCode("ILS");
        public static ICurrency IndianRupee => KnownCurrencyTable.FromIsoCode("INR");
        public static ICurrency IraqiDinar => KnownCurrencyTable.FromIsoCode("IQD");
        public static ICurrency IranianRial => KnownCurrencyTable.FromIsoCode("IRR");
        public static ICurrency IcelandicKróna => KnownCurrencyTable.FromIsoCode("ISK");
        public static ICurrency JamaicanDollar => KnownCurrencyTable.FromIsoCode("JMD");
        public static ICurrency JordanianDinar => KnownCurrencyTable.FromIsoCode("JOD");
        public static ICurrency JapaneseYen => KnownCurrencyTable.FromIsoCode("JPY");
        public static ICurrency KenyanShilling => KnownCurrencyTable.FromIsoCode("KES");
        public static ICurrency KyrgystaniSom => KnownCurrencyTable.FromIsoCode("KGS");
        public static ICurrency CambodianRiel => KnownCurrencyTable.FromIsoCode("KHR");
        public static ICurrency SouthKoreanWon => KnownCurrencyTable.FromIsoCode("KRW");
        public static ICurrency KuwaitiDinar => KnownCurrencyTable.FromIsoCode("KWD");
        public static ICurrency KazakhstaniTenge => KnownCurrencyTable.FromIsoCode("KZT");
        public static ICurrency LaotianKip => KnownCurrencyTable.FromIsoCode("LAK");
        public static ICurrency LebanesePound => KnownCurrencyTable.FromIsoCode("LBP");
        public static ICurrency SriLankanRupee => KnownCurrencyTable.FromIsoCode("LKR");
        public static ICurrency LibyanDinar => KnownCurrencyTable.FromIsoCode("LYD");
        public static ICurrency MoroccanDirham => KnownCurrencyTable.FromIsoCode("MAD");
        public static ICurrency MoldovanLeu => KnownCurrencyTable.FromIsoCode("MDL");
        public static ICurrency MacedonianDenar => KnownCurrencyTable.FromIsoCode("MKD");
        public static ICurrency MyanmarKyat => KnownCurrencyTable.FromIsoCode("MMK");
        public static ICurrency MongolianTugrik => KnownCurrencyTable.FromIsoCode("MNT");
        public static ICurrency MacanesePataca => KnownCurrencyTable.FromIsoCode("MOP");
        public static ICurrency MaldivianRufiyaa => KnownCurrencyTable.FromIsoCode("MVR");
        public static ICurrency MexicanPeso => KnownCurrencyTable.FromIsoCode("MXN");
        public static ICurrency MalaysianRinggit => KnownCurrencyTable.FromIsoCode("MYR");
        public static ICurrency NigerianNaira => KnownCurrencyTable.FromIsoCode("NGN");
        public static ICurrency NicaraguanCórdoba => KnownCurrencyTable.FromIsoCode("NIO");
        public static ICurrency NorwegianKrone => KnownCurrencyTable.FromIsoCode("NOK");
        public static ICurrency NepaleseRupee => KnownCurrencyTable.FromIsoCode("NPR");
        public static ICurrency NewZealandDollar => KnownCurrencyTable.FromIsoCode("NZD");
        public static ICurrency OmaniRial => KnownCurrencyTable.FromIsoCode("OMR");
        public static ICurrency PanamanianBalboa => KnownCurrencyTable.FromIsoCode("PAB");
        public static ICurrency PeruvianSol => KnownCurrencyTable.FromIsoCode("PEN");
        public static ICurrency PhilippinePiso => KnownCurrencyTable.FromIsoCode("PHP");
        public static ICurrency PakistaniRupee => KnownCurrencyTable.FromIsoCode("PKR");
        public static ICurrency PolishZloty => KnownCurrencyTable.FromIsoCode("PLN");
        public static ICurrency ParaguayanGuarani => KnownCurrencyTable.FromIsoCode("PYG");
        public static ICurrency QatariRial => KnownCurrencyTable.FromIsoCode("QAR");
        public static ICurrency RomanianLeu => KnownCurrencyTable.FromIsoCode("RON");
        public static ICurrency SerbianDinar => KnownCurrencyTable.FromIsoCode("RSD");
        public static ICurrency RussianRuble => KnownCurrencyTable.FromIsoCode("RUB");
        public static ICurrency RwandanFranc => KnownCurrencyTable.FromIsoCode("RWF");
        public static ICurrency SaudiRiyal => KnownCurrencyTable.FromIsoCode("SAR");
        public static ICurrency SwedishKrona => KnownCurrencyTable.FromIsoCode("SEK");
        public static ICurrency SingaporeDollar => KnownCurrencyTable.FromIsoCode("SGD");
        public static ICurrency SomaliShilling => KnownCurrencyTable.FromIsoCode("SOS");
        public static ICurrency SyrianPound => KnownCurrencyTable.FromIsoCode("SYP");
        public static ICurrency ThaiBaht => KnownCurrencyTable.FromIsoCode("THB");
        public static ICurrency TajikistaniSomoni => KnownCurrencyTable.FromIsoCode("TJS");
        public static ICurrency TurkmenistaniManat => KnownCurrencyTable.FromIsoCode("TMT");
        public static ICurrency TunisianDinar => KnownCurrencyTable.FromIsoCode("TND");
        public static ICurrency TurkishLira => KnownCurrencyTable.FromIsoCode("TRY");
        public static ICurrency TrinidadAndTobagoDollar => KnownCurrencyTable.FromIsoCode("TTD");
        public static ICurrency NewTaiwanDollar => KnownCurrencyTable.FromIsoCode("TWD");
        public static ICurrency UkrainianHryvnia => KnownCurrencyTable.FromIsoCode("UAH");
        public static ICurrency USDollar => KnownCurrencyTable.FromIsoCode("USD");
        public static ICurrency UruguayanPeso => KnownCurrencyTable.FromIsoCode("UYU");
        public static ICurrency UzbekistaniSom => KnownCurrencyTable.FromIsoCode("UZS");
        public static ICurrency VenezuelanBolívar => KnownCurrencyTable.FromIsoCode("VES");
        public static ICurrency VietnameseDong => KnownCurrencyTable.FromIsoCode("VND");
        public static ICurrency CentralAfricanCFAFranc => KnownCurrencyTable.FromIsoCode("XAF");
        public static ICurrency EastCaribbeanDollar => KnownCurrencyTable.FromIsoCode("XCD");
        public static ICurrency SpecialDrawingRights => KnownCurrencyTable.FromIsoCode("XDR");
        public static ICurrency WestAfricanCFAFranc => KnownCurrencyTable.FromIsoCode("XOF");
        public static ICurrency YemeniRial => KnownCurrencyTable.FromIsoCode("YER");
        public static ICurrency SouthAfricanRand => KnownCurrencyTable.FromIsoCode("ZAR");

        public static ICurrency FromISOCode(string code)
        {
            KnownCurrencyTable.EnsureCurrencyTable();
            return KnownCurrencyTable.CurrencyTable[code];
        }

        public static ICurrency FromName(string name)
        {
            KnownCurrencyTable.EnsureCurrencyTable();
            return KnownCurrencyTable.CurrencyTable.Values.FirstOrDefault(c => c.Name == name);
        }

        public static ICurrency FromRegion(RegionInfo region)
        {
            KnownCurrencyTable.EnsureCurrencyTable();
            return KnownCurrencyTable.CurrencyTable[region.ISOCurrencySymbol];
        }

        public static ICurrency FromCulture(CultureInfo culture)
        {
            KnownCurrencyTable.EnsureCurrencyTable();
            var temp = new Currency(culture);
            return temp != null ? KnownCurrencyTable.CurrencyTable.Values.FirstOrDefault(c => c.Name == temp.IsoCode) : null;
        }
    }
}
