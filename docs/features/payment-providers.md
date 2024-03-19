# Payment Providers

Orchard Core Commerce supports multiple payment providers and allows developers to extend it further with their own.

## Official payment providers

Each provider is a stand-alone feature you can turn on or off.

- [Stripe](stripe-payment.md): A production-ready provider for [stripe.com](https://stripe.com/).
- Dummy: A development-only provider that lets you click through the checkout without going off-site. Mainly used for UI testing.

## Creating your own

To create a custom payment provider your code must contain the following:

- An implementation of `IPaymentProvider` registered as a service.
- A shape type `Checkout{provider.Name}`, such as _CheckoutStripe.cshtml_ and _CheckoutDummy.cshtml_.

The `IPaymentProvider` contains implementable methods used by the `CheckoutController`. To learn more about the individual methods in the interface, check out the individual methods' XML documentation.

The shape has the payment button that will be displayed on the `~/checkout` screen. It's up to you to include the front-end logic that calls out to your payment processor and to provide a callback URL. For the latter you can use the `~/checkout/callback/{providerName}/{orderId?}` action. It handles some basic state checking and redirection, but otherwise lets you resolve the pending order using `IPaymentProvider.UpdateAndRedirectToFinishedOrderAsync()`.
