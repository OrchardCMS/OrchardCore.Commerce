import {
    createPaymentIntent,
    createSubscription,
    getPaymentDetails,
    getStripeConfirmParameters
} from "@/modules/payment/api";

export async function createConfirmationToken(stripe, elements): Promise {
    return stripe.createConfirmationToken({
        elements
    });
}

export async function confirmSetup(stripe, clientSecret, confirmParams, elements): Promise {
    const { error } = await stripe.confirmSetup({
        elements,
        clientSecret,
        confirmParams
    });

    return error;
}

// If elements and a confirmationTokenId are provided, there will be an error. Only one of them should be provided.
export async function confirmPayment(stripe, clientSecret, confirmParams, elements): Promise {
    const { error } = await stripe.confirmPayment({
        elements,
        clientSecret,
        confirmParams,
    });

    return error;
}

export async function startPayment(stripe, confirmationTokenId, mode, returnUrl, elements) : Promise {
    const paymentDetails = await getPaymentDetails();

    if (mode === 'subscription') {
        const { type, clientSecret } = await createSubscription(paymentDetails);
        await getStripeParametersAndConfirm(
            stripe,
            clientSecret,
            type,
            confirmationTokenId,
            null,
            returnUrl,
            elements);
    }
    else if (mode === 'payment') {
        // Create payment intent right before starting the payment.
        const { clientSecret, orderContentItemId } = await createPaymentIntent(paymentDetails);
        await getStripeParametersAndConfirm(
            stripe,
            clientSecret,
            null,
            confirmationTokenId,
            orderContentItemId,
            returnUrl,
            elements);
    }
    else {
        throw new Error('Invalid mode');
    }
}

async function getStripeParametersAndConfirm(stripe, clientSecret, type, confirmationTokenId, orderContentItemId, returnUrl, elements) {
    const confirmParams = await getStripeConfirmParameters(returnUrl, orderContentItemId);
    confirmParams.confirmation_token = confirmationTokenId;

    if (type === "setup") {
        return confirmSetup(stripe, clientSecret, confirmParams, elements);
    }
    else {
        return confirmPayment(stripe, clientSecret, confirmParams, elements);
    }
}

export function initializeStripe(stripe, total, mode = 'subscription') {
    const appearance = {
        theme: 'stripe',
    };

    // This amount won't be sent to the backend, it will be reevaluated in the backend. This is only for display purposes.
    const options = {
        mode: mode, // 'subscription' or 'payment'
        amount: total?.amount,
        currency: total?.currency?.currencyIsoCode?.toLowerCase(),
        appearance: appearance
    };

    const elements = stripe.elements(options);

    const paymentElementOptions = {
        layout: {
            type: 'accordion',
            defaultCollapsed: true,
            radios: false,
            spacedAccordionItems: true
        }
    };

    const paymentElement = elements.create("payment", paymentElementOptions);
    paymentElement.mount("#payment-element");
    return elements;
}
