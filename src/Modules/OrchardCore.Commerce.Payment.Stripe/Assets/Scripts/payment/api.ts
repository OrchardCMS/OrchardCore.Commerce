import httpClient from '@/httpClient';
import { loadStripe } from '@stripe/stripe-js';
import type { IPaymentDetails, ITotal } from "@/modules/payment/model";

export async function getStripePublicKey(): Promise {
    const api = new httpClient();
    const { data } = await api.get('api/checkout/stripe/public-key');
    return await loadStripe(data);
}

export async function getTotal(): Promise<ITotal> {
    const api = new httpClient();
    const { data } = await api.get('api/checkout/stripe/total/');
    return data;
}

export async function getPaymentDetails(): Promise<IPaymentDetails> {
    const api = new httpClient();
    const { data } = await api.get('api/payment-details');
    return data;
}

export async function savePaymentDetails(paymentDetails : IPaymentDetails): Promise<IPaymentDetails> {
    const api = new httpClient();
    const { data } = await api.post('api/payment-details', paymentDetails);
    return data;
}

export async function createPaymentIntent(paymentDetails : IPaymentDetails): Promise {
    const api = new httpClient();
    const orderPart = {
        Email: {
            Text: paymentDetails.email.text,
        },
        BillingAddress: {
            Address:{
                Name: `${paymentDetails.firstName.text} ${paymentDetails.lastName.text}`,
                Company: paymentDetails.company.text,
                StreetAddress1: paymentDetails.street.text,
                City: paymentDetails.city.text,
                PostalCode: paymentDetails.zipCode.text,
                Region: paymentDetails.country.text
            }
        }
    }

    const { data } = await api.post('api/checkout/stripe/payment-intent',{ orderPart });
    return data;
}

export async function createSubscription(paymentDetails : IPaymentDetails, customerId, priceIds): Promise {
    const api = new httpClient();
    const orderPart = {
        Email: {
            Text: paymentDetails.email.text,
        },
        BillingAddress: {
            Address:{
                Name: `${paymentDetails.firstName.text} ${paymentDetails.lastName.text}`,
                Company: paymentDetails.company.text,
                StreetAddress1: paymentDetails.street.text,
                City: paymentDetails.city.text,
                PostalCode: paymentDetails.zipCode.text,
                Region: paymentDetails.country.text
            }
        }
    }

    const { data } = await api.post('api/checkout/stripe/subscription',{ customerId, orderPart, priceIds });
    return data;
}

export async function getStripeConfirmParameters(returnUrl, orderId): Promise {
    const api = new httpClient();
    const { data } = await api.post('api/checkout/stripe/confirm-parameters', { returnUrl, orderId });
    return data;
}

export async function getConfirmationToken(confirmationTokenId): Promise {
    const api = new httpClient();
    const { data } = await api.get('api/checkout/stripe/confirmation-token?confirmationTokenId=' + confirmationTokenId);
    return data;
}
