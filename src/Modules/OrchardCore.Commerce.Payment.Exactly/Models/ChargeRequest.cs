using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.MoneyDataType.Extensions;
using OrchardCore.Commerce.Payment.Exactly.Controllers;
using OrchardCore.Users.Models;
using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Exactly.Models;

public class ChargeRequest : IExactlyRequestAttributes, IExactlyAmount
{
    [JsonIgnore]
    public string Type => "charge";

    public string ProjectId { get; set; }
    public string PaymentMethod { get; set; } = "card";
    public string Amount { get; set; }
    public string Currency { get; set; }
    public string ReferenceId { get; set; }
    public string CustomerDescription { get; set; }
    public string ReturnUrl { get; set; }
    public string CustomerId { get; set; }
    public string Email { get; set; }
    public bool TokenizeSource { get; set; } = true;
    public int Lifetime { get; set; } = 3600;
    public object Meta { get; set; }

    public ChargeRequest()
    {
    }

    public ChargeRequest(OrderPart orderPart, User user, string projectId, string tenantId, Uri returnUrl)
    {
        if (!returnUrl.IsAbsoluteUri) throw new ArgumentException("The return URL must be absolute.", nameof(returnUrl));

        ProjectId = projectId;
        ReferenceId = $"{tenantId}-{orderPart.OrderId.Text}";
        CustomerDescription = string.Join(", ", orderPart.Charges.Select(payment => $"{payment.Amount} × {payment.ChargeText}"));
        ReturnUrl = returnUrl.AbsoluteUri;
        CustomerId = user.UserId;
        Email = user.Email;
        Meta = orderPart;

        this.SetAmount(orderPart.Charges.Select(payment => payment.Amount).Sum());
    }

    public static implicit operator ExactlyRequest<ChargeRequest>(ChargeRequest attributes) =>
        new() { Attributes = attributes };

    public static async Task<ChargeRequest> CreateUserAsync(OrderPart orderPart, HttpContext context)
    {
        var provider = context.RequestServices;
        var returnurl = context.ActionTask<ExactlyController>(controller => controller.Middleware());

        return new ChargeRequest(
            orderPart,
            await provider.GetRequiredService<IUserService>().GetFullUserAsync(context.User),
            provider.GetRequiredService<IOptionsSnapshot<ExactlySettings>>().Value.ProjectId,
            context.Request.Host.Host,
            new Uri(new Uri(context.Request.GetDisplayUrl()), returnurl));
    }
}
