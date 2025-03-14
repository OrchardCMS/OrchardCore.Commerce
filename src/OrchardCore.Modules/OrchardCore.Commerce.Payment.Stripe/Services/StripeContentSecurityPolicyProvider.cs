using Lombiq.HelpfulLibraries.AspNetCore.Security;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Lombiq.HelpfulLibraries.AspNetCore.Security.ContentSecurityPolicyDirectives;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public class StripeContentSecurityPolicyProvider : IContentSecurityPolicyProvider
{
    public ValueTask UpdateAsync(IDictionary<string, string> securityPolicies, HttpContext context)
    {
        securityPolicies[ScriptSrc] = ContentSecurityPolicyProvider
            .GetDirective(securityPolicies, ScriptSrc)
            .MergeWordSets("https://js.stripe.com/");

        securityPolicies[FrameSrc] = ContentSecurityPolicyProvider
            .GetDirective(securityPolicies, FrameSrc)
            .MergeWordSets("https://js.stripe.com/");

        return ValueTask.CompletedTask;
    }
}
