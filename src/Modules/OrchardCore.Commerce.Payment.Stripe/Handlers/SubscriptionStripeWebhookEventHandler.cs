using Lombiq.HelpfulLibraries.OrchardCore.Users;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Services;
using OrchardCore.Commerce.Services;
using Stripe;
using System.Text.Json;
using System.Threading.Tasks;
using static Stripe.Events;

namespace OrchardCore.Commerce.Payment.Stripe.Handlers;

public class SubscriptionStripeWebhookEventHandler : IStripeWebhookEventHandler
{
    private readonly ICachingUserManager _cachingUserManager;
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<SubscriptionStripeWebhookEventHandler> _logger;
    private readonly IStripeSubscriptionService _stripeSubscriptionService;

    public SubscriptionStripeWebhookEventHandler(
        ICachingUserManager cachingUserManager,
        ISubscriptionService subscriptionService,
        IStripeSubscriptionService stripeSubscriptionService,
        ILogger<SubscriptionStripeWebhookEventHandler> logger)
    {
        _cachingUserManager = cachingUserManager;
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

                // Get current subscription if exists, if exists do not override start date
                // Handle case when subscription isn't active, or someone else payed for the tenant.
                // Might be a good idea to do it in an officefreund own handler
                var subscriptionPart = new SubscriptionPart();
                subscriptionPart.UserId.Text = user.UserId;
                subscriptionPart.Status.Text = SubscriptionStatuses.Active;
                subscriptionPart.EndDateUtc.Value = invoice.PeriodEnd;
                subscriptionPart.PaymentProviderName.Text = StripePaymentProvider.ProviderName;
                subscriptionPart.IdInPaymentProvider.Text = invoice.SubscriptionId;

                var stripeSubscription = await _stripeSubscriptionService.GetSubscriptionAsync(invoice.SubscriptionId, options: null);
                subscriptionPart.Metadata = stripeSubscription.Metadata;

                await _subscriptionService.CreateOrUpdateSubscriptionAsync(invoice.SubscriptionId, subscriptionPart);
            }
        }
    }
}
