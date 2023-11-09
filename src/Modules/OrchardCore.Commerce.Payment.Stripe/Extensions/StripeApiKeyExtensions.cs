using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.Payment.Stripe.Services;
using System;

namespace OrchardCore.Commerce.Payment.Stripe.Extensions;
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
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "The Stripe secret key could not be decrypted. It may have been encrypted using a different key.");
            return null;
        }
    }
}
