#nullable enable
using Lombiq.HelpfulLibraries.OrchardCore.Users;
using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Indexes;
using OrchardCore.Commerce.Payment.Stripe.Models;
using Stripe;
using Stripe.Checkout;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using YesSql;
using ISession = YesSql.ISession;
using Session = Stripe.Checkout.Session;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public class StripeSessionService : IStripeSessionService
{
    private readonly SessionService _sessionService = new();
    private readonly IRequestOptionsService _requestOptionsService;
    private readonly IHttpContextAccessor _hca;
    private readonly ISession _session;
    private readonly ICachingUserManager _cachingUserManager;
    private readonly IEnumerable<IStripeSessionEventHandler> _stripeSessionEventHandlers;

    public StripeSessionService(
        IRequestOptionsService requestOptionsService,
        IHttpContextAccessor httpContextAccessor,
        ISession session,
        ICachingUserManager cachingUserManager,
        IEnumerable<IStripeSessionEventHandler> stripeSessionEventHandlers)
    {
        _requestOptionsService = requestOptionsService;
        _hca = httpContextAccessor;
        _session = session;
        _cachingUserManager = cachingUserManager;
        _stripeSessionEventHandlers = stripeSessionEventHandlers;
    }

    public async Task<Session> CreateSessionAsync(SessionCreateOptions options) =>
        await _sessionService.CreateAsync(
            options,
            await _requestOptionsService.SetIdempotencyKeyAsync(),
            cancellationToken: _hca.HttpContext.RequestAborted);

    // Create session content item and save it to the database
    public async Task<StripeSessionDataSave> SaveSessionDataAsync(Customer customer, Session session)
    {
        var user = await _cachingUserManager.GetUserByEmailAsync(customer.Email);
        if (user == null)
        {
            return new StripeSessionDataSave { Errors = ["User not found with email: " + customer.Email] };
        }

        var sessionData = await GetSessionDataQuery(user.UserId, session.InvoiceId, session.Id).FirstOrDefaultAsync();
        sessionData ??= new StripeSessionData();
        sessionData.UserId = user.UserId;
        sessionData.StripeSessionId = session.Id;
        sessionData.StripeSessionUrl = session.Url;
        sessionData.StripeInvoiceId = session.InvoiceId;
        sessionData.StripeCustomerId = customer.Id;
        sessionData.SerializedAdditionalData = JsonSerializer.Serialize(session.Metadata);

        // Here you can override to the session data that will be saved. And e.g. give additional data in the serializeddata.
        await _stripeSessionEventHandlers.AwaitEachAsync(handler =>
            handler.StripeSessionDataCreatingAsync(sessionData, session, customer));

        await _session.SaveAsync(sessionData);

        return new StripeSessionDataSave { StripeSessionData = sessionData };
    }

    public Task<IEnumerable<StripeSessionData>> GetAllSessionDataAsync(string userId, string invoiceId, string sessionId) =>
        GetSessionDataQuery(userId, invoiceId, sessionId).ListAsync();

    // Get session data by invoice id
    public Task<StripeSessionData> GetFirstSessionDataByInvoiceIdAsync(string invoiceId) =>
        GetSessionDataQuery(userId: string.Empty, invoiceId, sessionId: string.Empty).FirstOrDefaultAsync();

    public IQuery<StripeSessionData, StripeSessionDataIndex> GetSessionDataQuery(string userId, string invoiceId, string sessionId)
    {
        var query = _session.Query<StripeSessionData, StripeSessionDataIndex>();
        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(item => item.UserId == userId);
        }

        if (!string.IsNullOrEmpty(invoiceId))
        {
            query = query.Where(item => item.StripeInvoiceId == invoiceId);
        }

        if (!string.IsNullOrEmpty(sessionId))
        {
            query = query.Where(item => item.StripeSessionId == sessionId);
        }

        return query;
    }
}
