# Exactly® Payment

Orchard Core Commerce supports multiple [payment providers](payment-providers.md). [Exactly®](https://exactly.com/) is one of the officially included ones. Unlike [our Stripe implementation](stripe-payment.md), it uses redirects to send you to a payment interface on their domain.

To start using, follow these steps:

1. Sign up to Exactly®. Please be sure to use [this link](https://application.exactly.com/?utm_source=partner&utm_medium=kirill&utm_campaign=LOMBIQ) to create your account. That way [Lombiq](https://lombiq.com) (the steward of the Orchard Core Commerce project) will get a commission on the payment fees, which helps cover some of the development costs of OCC. This is at no cost to you; the fees you pay are the same either way.
2. Once they respond, ask your contact person for the following:
   - Whitelist for your web domain.
   - If needed, a sandbox project you can use for testing.
   - A live project for your final site.
3. Go to <https://dashboard.exactly.com/projects/>.
4. Take note of your _Project ID_ and your _Project API key_ (both are GUID style hexadecimal strings).
5. On your Orchard Core tenant go to Admin dashboard → Configuration → Features.
6. Make sure the _Orchard Core Commerce - Payment - Exactly_ feature is enabled.
7. Go to Admin dashboard → Configuration → Commerce → Exactly API.
8. Fill out the _Project ID_ and _API key_ fields.
9. Save and then click the _Verify currently saved API configuration_ button to test it. This will create a new transaction you can check on <https://dashboard.exactly.com/transactions>.

Once you have set up the site configuration, an additional _Pay with exactly_ button will appear during checkout.

> ℹ At the time of writing callback URLs targeting _localhost_ are not supported. If you want to test your site locally, we suggest adding a whitelisted domain to your [hosts file](https://en.wikipedia.org/wiki/Hosts_(file)). The address doesn't have to be accessible from their server so this approach is safer than exposing your machine via port forwarding or tunneling.

## Cards

There are available test cards for the sandbox environment that can be found in [Exactly's documentation](https://exactly.com/docs/api#tag/Transactions/operation/createTransaction). Some of these test card numbers are commonly used by other payment providers as well.

| Brand      | Number              | CVC          | Date            | Result                                           |
|------------|---------------------|--------------|-----------------|--------------------------------------------------|
| Visa       | 4000 0000 0000 7775 | Any 3 digits | Any future date | the card has insufficient funds                  |
| Visa       | 4000 0000 0000 3220 | Any 3 digits | Any future date | success during 3DS Auth (3DS is always expected) |
| Visa       | 4000 0084 0000 1280 | Any 3 digits | Any future date | the card fails 3DS Auth (3DS is always expected) |
| Mastercard | 5555 5555 5555 4444 | Any 3 digits | Any future date | Mastercard test card                             |

> ⚠ The sandbox environment only supports EUR and USD currencies. If payment is attempted with anything else, it will display a _403.21: Unsupported currency_ error.

## Technical overview

As mentioned above, this module uses redirects to communicate with the payment processor. This means the OrchardCore.Commerce site never sees the buyer's payment information, which avoids potential liability and improves buyer confidence. Here is a broad overview of what happens when you click on the _Pay with exactly_ button:

- JS script sends a POST request that only contains the contents of the checkout page (i.e. addresses).
- C# backend creates a new Order content item from the checkout data and the stored shopping cart.
- C# backend sends a POST request to the Exactly API including the order total and the return URL.
- JS script gets a redirect URL (on Exactly's domain) as response, then navigates there.
- Payment continues on Exactly, then redirects back to the return URL.
- C# backend validates the transaction state, updates Order and redirects to the Success page.
