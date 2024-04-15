# Exactly Payment

Orchard Core Commerce supports multiple [payment providers](payment-providers.md). [Exactly](https://exactly.com/) is one of the officially included ones. Unlike [our Stripe implementation](stripe-payment.md), it uses redirects to send you to a payment interface on their domain.

To start using, follow these steps:
1. Sign up to Exactly. Please be sure to use [this link](https://application.exactly.com/?utm_source=partner&utm_medium=kirill&utm_campaign=LOMBIQ) to create your account. That way [Lombiq](https://lombiq.com) (the steward of the Orchard Core Commerce project) will get a commission on the payment fees, which helps cover some of the development costs of OCC. This is at no cost to you; the fees you pay are the same either way.
2. Once they respond, ask your contact person for the following:
   - Whitelist for your web domain.
   - If needed, a sandbox project you can use for testing.
   - A live project for your final site.
3. Go to <https://dashboard.exactly.com/projects/>.
4. Take note of your _Project ID_ and your _Project API key_ (both are GUID style hexadecimal strings).
5. On your Orchard Core tenant go to Admin dashboard > Configuration > Features. 
6. Make sure the _Orchard Core Commerce - Payment - Exactly_ feature is enabled.
7. Go to Admin dashboard > Configuration > Commerce > Exactly API.
8. Fill out the _Project ID_ and _API key_ fields.
9. Save and then click the _Verify currently saved API configuration_ button to test it. This will create a new transaction you can check on <https://dashboard.exactly.com/transactions>.

Once you have set up the site configuration, an additional _Pay with Exactly_ button will appear during checkout.

> ℹ At the time of writing callback URLs targeting _localhost_ are not supported. If you want to test your site locally, we suggest adding a whitelisted domain to your [hosts file](https://en.wikipedia.org/wiki/Hosts_(file)). The address doesn't have to be accessible from their server so this approach is safer than exposing your machine via port forwarding or tunneling..

### Cards

There are available test cards that can be found in [Stripe's documentation](https://stripe.com/docs/testing).

There are multiple test cards that can simulate any scenario, including error codes. Here are two examples:

| Brand      | Number              | CVC          | Date            | Result                                           |
|------------|---------------------|--------------|-----------------|--------------------------------------------------|
| Visa       | 4000 0000 0000 7775 | Any 3 digits | Any future date | the card has insufficient funds                  |
| Visa       | 4000 0000 0000 3220 | Any 3 digits | Any future date | success during 3DS Auth (3DS is always expected) |
| Visa       | 4000 0084 0000 1280 | Any 3 digits | Any future date | the card fails 3DS Auth (3DS is always expected) |
| Mastercard | 5555 5555 5555 4444 | Any 3 digits | Any future date | Mastercard test card                             |
