<template>
<form v-if="total" id="payment-form">
    <div id="payment-element">
        <!--Stripe.js injects the Payment Element-->
    </div>
    <button id="submit" @click="handleSubmit">
        <div class="spinner hidden" id="spinner"></div>
        <span id="button-text">{{ $t('stripe.actions.toConfirmationPage') }}</span>
    </button>
    <div id="payment-message" class="hidden"></div>
</form>
<div v-else>
    <span>{{ $t('stripe.empty') }}</span>
</div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { PageNames } from "@/plugins/router/constants";
import {
    getTotal,
    getStripePublicKey,
    createConfirmationToken,
    initializeStripe,
} from '@/modules/payment';

const mode = 'subscription';
const router = useRouter();

const stripe = await getStripePublicKey();
const total = await getTotal();
let elements;

onMounted(async () => {
    // If there is no total, should not display the payment form.
    if (!total){
        return;
    }

    // Initialize the payment form without creating a payment intent in Stripe
    elements = await initializeStripe(stripe, total, mode);
})

const handleSubmit = async () => {
    setLoading(true);

    // Trigger form validation and wallet collection
    const {error: submitError} = await elements.submit();
    if (submitError) {
        setLoading(false);
        return;
    }

    // Create confirmation token
    const { error, confirmationToken } = await createConfirmationToken(stripe, elements);

    if (error) {
        await showError(error);
        setLoading(false);
        return;
    }

    router.push({ name: PageNames.PaymentConfirm, query: { confirmationTokenId: confirmationToken.id, mode: mode }})
}

// ------- UI helpers -------
function showError(error) {
    // This point will only be reached if there is an immediate error when
    // confirming the payment. Otherwise, your customer will be redirected to
    // your `return_url`. For some payment methods like iDEAL, your customer will
    // be redirected to an intermediate site first to authorize the payment, then
    // redirected to the `return_url`.
    if (error?.type === "card_error" || error?.type === "validation_error") {
        showMessage(error.message);
    } else {
        showMessage("An unexpected error occurred.");
    }
}

function showMessage(messageText) {
    const messageContainer = document.querySelector("#payment-message");

    messageContainer.classList.remove("hidden");
    messageContainer.textContent = messageText;

    setTimeout(function () {
        messageContainer.classList.add("hidden");
        messageContainer.textContent = "";
    }, 4000);
}

// Show a spinner on payment submission
function setLoading(isLoading) {
    if (isLoading) {
        // Disable the button and show a spinner
        document.querySelector("#submit").disabled = true;
        document.querySelector("#spinner").classList.remove("hidden");
        document.querySelector("#button-text").classList.add("hidden");
    } else {
        document.querySelector("#submit").disabled = false;
        document.querySelector("#spinner").classList.add("hidden");
        document.querySelector("#button-text").classList.remove("hidden");
    }
}
</script>
