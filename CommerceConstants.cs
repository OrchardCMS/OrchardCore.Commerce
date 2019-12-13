namespace OrchardCore.Commerce
{
    public static class CommerceConstants
    {
        public static class Features
        {
            public const string Core = "OrchardCore.Commerce";
            public const string SessionCartStorage = "OrchardCore.Commerce.SessionCartStorage";
            public const string PriceBooks = "OrchardCore.Commerce.PriceBooks";
            public const string PriceBooksByRole = "OrchardCore.Commerce.PriceBooks.ByRole";
            public const string PriceBooksByUser = "OrchardCore.Commerce.PriceBooks.ByUser";
        }

        public static class ContentTypes
        {
            public const string PriceBook = "PriceBook";
            public const string PriceBookEntry = "PriceBookEntry";
            public const string PriceBookByUser = "PriceBookByUser";
            public const string PriceBookByRole = "PriceBookByRole";
        }
    }
}
