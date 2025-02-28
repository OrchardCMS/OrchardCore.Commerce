using Lombiq.HelpfulLibraries.OrchardCore.Users;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Services;
using OrchardCore.Commerce.Services;
using OrchardCore.ContentManagement;
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
    private readonly IContentManager _contentManager;

    public SubscriptionStripeWebhookEventHandler(
        ICachingUserManager cachingUserManager,
        ISubscriptionService subscriptionService,
        IStripeSubscriptionService stripeSubscriptionService,
        IContentManager contentManager,
        ILogger<SubscriptionStripeWebhookEventHandler> logger)
    {
        _cachingUserManager = cachingUserManager;
        _subscriptionService = subscriptionService;
        _stripeSubscriptionService = stripeSubscriptionService;
        _contentManager = contentManager;
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
                    _logger.LogWarning(
                        "User not found for email {Email}, while invoice was paid. Invoice data: {InvoiceData}",
                        invoice.CustomerEmail,
                        JsonSerializer.Serialize(invoice));
                }

                var subscriptionPart = new SubscriptionPart();
                subscriptionPart.UserId.Text = user?.UserId;
                subscriptionPart.Status.Text = SubscriptionStatuses.Active;

                // invoice.PeriodEnd doesn't show the current period, see Stripe docs:
                // https://docs.stripe.com/api/invoices/object#invoice_object-period_end
                // "End of the usage period during which invoice items were added to this invoice. This looks back one
                // period for a subscription invoice. Use the line item period to get the service period for each price."
                subscriptionPart.EndDateUtc.Value = invoice.Lines.Data.Find(data => !data.Proration)?.Period.End;
                subscriptionPart.PaymentProviderName.Text = StripePaymentProvider.ProviderName;
                subscriptionPart.IdInPaymentProvider.Text = invoice.SubscriptionId;

                var stripeSubscription = await _stripeSubscriptionService.GetSubscriptionAsync(invoice.SubscriptionId, options: null);
                subscriptionPart.Metadata = stripeSubscription.Metadata;
                subscriptionPart.StartDateUtc.Value = stripeSubscription.StartDate;

                await _subscriptionService.CreateOrUpdateSubscriptionAsync(invoice.SubscriptionId, subscriptionPart);
            }
        }
        else if (stripeEvent.Type is CustomerSubscriptionUpdated or
                 CustomerSubscriptionDeleted or
                 CustomerSubscriptionResumed or
                 CustomerSubscriptionPaused)
        {
            var stripeSubscription = stripeEvent.Data.Object as Subscription;
            // Get the subscription content item for this subscription and set its status to the new status.
            var subscription = await _subscriptionService.GetSubscriptionAsync(stripeSubscription!.Id);
            if (subscription != null)
            {
                subscription.Alter<SubscriptionPart>(part =>
                {
                    part.Status.Text = stripeSubscription.Status;
                    part.EndDateUtc.Value = stripeSubscription.CurrentPeriodEnd;
                });

                await _contentManager.UpdateAsync(subscription);
            }
        }
    }
}
