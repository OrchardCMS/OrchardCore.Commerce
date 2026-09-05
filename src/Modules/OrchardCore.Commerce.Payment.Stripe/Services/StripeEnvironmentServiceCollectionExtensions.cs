using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using System;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

internal static class StripeEnvironmentServiceCollectionExtensions
{
    public static IServiceCollection AddStripeEnvironmentServices(this IServiceCollection services)
    {
        foreach (var environment in new[] { PaymentEnvironment.Production, PaymentEnvironment.Sandbox })
        {
            services.AddKeyedScoped<IRequestOptionsService>(
                environment,
                (provider, _) => ActivatorUtilities.CreateInstance<RequestOptionsService>(provider, environment));

            services.AddKeyedScoped<IStripePaymentIntentService>(
                environment,
                (provider, _) => ActivatorUtilities.CreateInstance<StripePaymentIntentService>(
                    provider,
                    environment,
                    provider.GetRequiredKeyedService<IRequestOptionsService>(environment)));

            services.AddKeyedScoped<IStripeCustomerService>(
                environment,
                (provider, _) => ActivatorUtilities.CreateInstance<StripeCustomerService>(
                    provider,
                    provider.GetRequiredKeyedService<IRequestOptionsService>(environment)));

            services.AddKeyedScoped<IStripeSessionService>(
                environment,
                (provider, _) => ActivatorUtilities.CreateInstance<StripeSessionService>(
                    provider,
                    provider.GetRequiredKeyedService<IRequestOptionsService>(environment)));

            services.AddKeyedScoped<IStripeSubscriptionService>(
                environment,
                (provider, _) => ActivatorUtilities.CreateInstance<StripeSubscriptionService>(
                    provider,
                    provider.GetRequiredKeyedService<IRequestOptionsService>(environment)));

            services.AddKeyedScoped<IStripeConfirmationTokenService>(
                environment,
                (provider, _) => ActivatorUtilities.CreateInstance<StripeConfirmationTokenService>(
                    provider,
                    provider.GetRequiredKeyedService<IRequestOptionsService>(environment)));

            services.AddKeyedScoped<IStripePaymentService>(
                environment,
                (provider, _) => ActivatorUtilities.CreateInstance<StripePaymentService>(
                    provider,
                    environment,
                    provider.GetRequiredKeyedService<IStripePaymentIntentService>(environment)));
        }

        services.AddScoped<IPaymentProvider>(provider =>
            CreatePaymentProvider(provider, PaymentEnvironment.Production));
        services.AddScoped<IPaymentProvider>(provider =>
            CreatePaymentProvider(provider, PaymentEnvironment.Sandbox));

        return services;
    }

    private static StripePaymentProvider CreatePaymentProvider(
        IServiceProvider provider,
        PaymentEnvironment environment) =>
        ActivatorUtilities.CreateInstance<StripePaymentProvider>(
            provider,
            environment,
            provider.GetRequiredKeyedService<IStripePaymentService>(environment),
            provider.GetRequiredKeyedService<IStripePaymentIntentService>(environment));
}
