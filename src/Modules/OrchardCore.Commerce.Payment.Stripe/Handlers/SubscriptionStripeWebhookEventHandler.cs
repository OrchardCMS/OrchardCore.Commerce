using Lombiq.HelpfulLibraries.OrchardCore.Users;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Services;
using OrchardCore.Commerce.Services;
using OrchardCore.ContentManagement;
using Stripe;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static Stripe.EventTypes;

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
            if (stripeEvent.Data.Object is Invoice { Status: "paid" } invoice)
            {
                var user = await _cachingUserManager.GetUserByEmailAsync(invoice.CustomerEmail);
                if (user == null)
                {
                    _logger.LogWarning(
                        "User not found for email {Email}, while invoice was paid. Invoice data: {InvoiceData}",
                        invoice.CustomerEmail,
                        JsonSerializer.Serialize(invoice));
                }

                var subscriptionId = invoice.Parent.SubscriptionDetails.SubscriptionId;
                var stripeSubscription = await _stripeSubscriptionService.GetSubscriptionAsync(subscriptionId);
                var subscriptionPart = new SubscriptionPart
                {
                    UserId = { Text = user?.UserId },
                    Status = { Text = SubscriptionStatuses.Active },

                    // invoice.PeriodEnd doesn't show the current period, see Stripe docs:
                    // https://docs.stripe.com/api/invoices/object#invoice_object-period_end
                    // "End of the usage period during which invoice items were added to this invoice. This looks back
                    // one period for a subscription invoice. Use the line item period to get the service period for
                    // each price."
                    EndDateUtc =
                    {
                        Value = invoice
                            .Lines
                            .Data
                            .Find(data => data.Parent?.InvoiceItemDetails?.Proration != true)?
                            .Period
                            .End,
                    },
                    PaymentProviderName = { Text = StripePaymentProvider.ProviderName },
                    IdInPaymentProvider = { Text = subscriptionId },

                    Metadata = stripeSubscription.Metadata,
                    StartDateUtc = { Value = stripeSubscription.StartDate },
                };

                await _subscriptionService.CreateOrUpdateSubscriptionAsync(subscriptionId, subscriptionPart);
            }
        }
        else if (stripeEvent.Type is CustomerSubscriptionUpdated or
                 CustomerSubscriptionDeleted or
                 CustomerSubscriptionResumed or
                 CustomerSubscriptionPaused)
        {
            var stripeSubscription = (Subscription)stripeEvent.Data.Object;
            // Get the subscription content item for this subscription and set its status to the new status.
            var subscription = await _subscriptionService.GetSubscriptionAsync(stripeSubscription!.Id);
            if (subscription != null)
            {
                subscription.Alter<SubscriptionPart>(part =>
                {
                    part.Status.Text = stripeSubscription.Status;
                    part.EndDateUtc.Value = stripeSubscription.Items.Max(item => item.CurrentPeriodEnd);
                });

                await _contentManager.UpdateAsync(subscription);
            }
        }
    }
}
