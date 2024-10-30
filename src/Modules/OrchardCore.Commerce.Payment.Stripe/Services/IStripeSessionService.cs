using OrchardCore.Commerce.Payment.Stripe.Models;
using Stripe;
using Stripe.Checkout;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public interface IStripeSessionService
{
    Task<Session> CreateSessionAsync(SessionCreateOptions options);

    Task<StripeSessionDataSave> SaveSessionDataAsync(Customer customer, Session session);

    Task<IEnumerable<StripeSessionData>> GetAllSessionDataAsync(string userId, string invoiceId, string sessionId);

    Task<StripeSessionData> GetFirstSessionDataByInvoiceIdAsync(string invoiceId);
}
