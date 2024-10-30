using Lombiq.HelpfulLibraries.OrchardCore.Users;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Services;
using OrchardCore.Modules;
using Stripe;
using System.Text.Json;
using System.Threading.Tasks;
using static Stripe.Events;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public class SubscriptionStripeWebhookEventHandler : IStripeWebhookEventHandler
{
    private readonly ICachingUserManager _cachingUserManager;
    private readonly IClock _clock;
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<SubscriptionStripeWebhookEventHandler> _logger;
    private readonly IStripeSubscriptionService _stripeSubscriptionService;

    public SubscriptionStripeWebhookEventHandler(
        ICachingUserManager cachingUserManager,
        IClock clock,
        ISubscriptionService subscriptionService,
        IStripeSubscriptionService stripeSubscriptionService,
        ILogger<SubscriptionStripeWebhookEventHandler> logger)
    {
        _cachingUserManager = cachingUserManager;
        _clock = clock;
        _subscriptionService = subscriptionService;
        _stripeSubscriptionService = stripeSubscriptionService;
        _logger = logger;
    }

    public async Task ReceivedStripeEventAsync(Event stripeEvent)
    {
        if (stripeEvent.Type == InvoicePaid)
        {
            var invoice = stripeEvent.Data.Object as Invoice;
            if (invoice?.Status == "paid")
            {
                var user = await _cachingUserManager.GetUserByEmailAsync(invoice.CustomerEmail);
                if (user == null)
                {
                    _logger.LogError(
                        "User not found for email {Email}, while invoice was payed. Invoice data: {InvoiceData}",
                        invoice.CustomerEmail,
                        JsonSerializer.Serialize(invoice));
                    return;
                }

                // Get session data for the invoice Id
                var subscriptionPart = new SubscriptionPart();
                subscriptionPart.UserId.Text = user.UserId;
                subscriptionPart.Status.Text = SubscriptionStatuses.Active;
                subscriptionPart.StartDateUtc.Value = _clock.UtcNow;
                subscriptionPart.EndDateUtc.Value = invoice.PeriodEnd;
                subscriptionPart.PaymentProviderName.Text = "Stripe";
                subscriptionPart.IdInPaymentProvider.Text = invoice.SubscriptionId;

                var stripeSubscription = await _stripeSubscriptionService.GetSubscriptionAsync(invoice.SubscriptionId, options: null);
                subscriptionPart.Metadata = stripeSubscription.Metadata;

                await _subscriptionService.CreateOrUpdateActiveSubscriptionAsync(invoice.SubscriptionId, subscriptionPart);
            }
        }
    }
}
