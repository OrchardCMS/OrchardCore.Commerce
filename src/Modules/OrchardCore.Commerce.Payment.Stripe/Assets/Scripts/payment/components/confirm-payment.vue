<template>
    <form id="confirmation-form">
        <button id="submit">
            <div class="spinner hidden" id="spinner"></div>
            <span id="button-text">{{ $t('stripe.actions.startPayment') }}</span>
        </button>
        <div id="confirmation-message" class="hidden"></div>
    </form>
</template>
<script setup lang="ts">
import { onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import {
    getStripePublicKey,
    startPayment,
} from '@/modules/payment';
import { PageNames } from "@/plugins/router/constants";

const route = useRoute();
const router = useRouter();

const confirmationTokenId = route.query.confirmationTokenId;
const mode = route.query.mode;
const stripe = await getStripePublicKey();
const handleSubmit = async (event) => {
    event.preventDefault();
    setLoading(true);

    // Redirect URL after payment
    const completePage = router.resolve({ name: PageNames.PaymentComplete })?.href;
    const returnUrl = window.location.origin + completePage;

    // Start payment
    // If confirmation token is provided, the elements won't be used because the confirmation token provides the
    // information the elements would also.
    const paymentResult = await startPayment(stripe, confirmationTokenId, mode, returnUrl);

    // If the result is successful we got redirected, so if we get here there was an error.
    await showError(paymentResult);
    setLoading(false);
}

onMounted(async () => {
    document
        .querySelector("#confirmation-form")
        .addEventListener("submit", handleSubmit);
})

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
