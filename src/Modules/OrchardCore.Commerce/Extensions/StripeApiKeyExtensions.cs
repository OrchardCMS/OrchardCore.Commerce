using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.Services;

namespace OrchardCore.Commerce.Extensions;
public static class StripeApiKeyExtensions
{
    public static string DecryptStripeApiKey(
        this string encryptedKey, IDataProtectionProvider dataProtectionProvider, ILogger logger
)
    {
        if (string.IsNullOrWhiteSpace(encryptedKey))
        {
            return null;
        }

        try
        {
            var protector = dataProtectionProvider.CreateProtector(nameof(StripeApiSettingsConfiguration));
            return protector.Unprotect(encryptedKey);
        }
        catch
        {
            logger.LogError("The Stripe secret key could not be decrypted. It may have been encrypted using a different key.");
            return null;
        }
    }
}
