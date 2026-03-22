namespace OrchardCore.Commerce;

public static class CommerceConstants
{
    public static class Features
    {
        public const string Core = "OrchardCore.Commerce";
        public const string SessionCartStorage = $"{Core}.{nameof(SessionCartStorage)}";
        public const string CurrencySettingsSelector = $"{Core}.{nameof(CurrencySettingsSelector)}";
        public const string Subscription = $"{Core}.{nameof(Subscription)}";
    }
}
