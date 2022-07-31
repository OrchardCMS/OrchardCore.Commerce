// 'stripe' is not defined
// It is in the view and we have to pass an API key to it there.
// eslint-disable-next-line
const stripeElements = stripe.elements();
const errorContainer = document.querySelector('.error-message');
const form = document.querySelector('.card-payment-form');
const submitButton = form.querySelector('button[type="submit"]');
let formElements = form.elements;

const card = stripeElements.create('card', {
    style: {
        base: {
            fontWeight: '500',
            fontSize: '20px',
        },
    },
});

const placeOfCard = document.querySelector('#card-payment-form_card');

const fetchErrorText = 'There was an error during fetching!';

function disableInputs() {
    for (let i = 0, length = formElements.length; i < length; ++i) {
        formElements[i].readOnly = true;
    }

    card.update({ disabled: true });

    submitButton.disabled = true;
}

function displayError(error) {
    if (Object.prototype.hasOwnProperty.call(error, 'message')) {
        errorContainer.textContent = error.message;
    }
    else {
        errorContainer.textContent = error;
    }

    // Enable inputs.
    for (let i = 0, length = formElements.length; i < length; ++i) {
        formElements[i].readOnly = false;
    }
    card.update({ disabled: false });

    submitButton.disabled = false;
}

function fetchPay(fetchBody) {
    return fetch('/pay', {
        method: 'POST',
        headers: {
            Accept: 'application/json',
            'Content-Type': 'application/json',
        },
        body: fetchBody,
    });
}

function handleServerResponse(response) {
    const error = response.error;

    // Show error in payment form.
    if (error) {
        displayError(error);
    }
    else if (response.requires_action) {
        // Use Stripe.js to handle required card action (like 3DS authentication).
        // eslint-disable-next-line
        stripe.handleCardAction(response.payment_intent_client_secret)
            // 'handleStripeJsResult' was used before it was defined.
            // Since handleServerResponse and handleStripeJsResult are used by each other, one has to be the second.
            // eslint-disable-next-line
            .then(handleStripeJsResult);
    }
    else {
        // Show success message.
        window.location.href = '/success';
    }
}

function handleStripeJsResult(result) {
    const error = result.error;

    // Show error in payment form.
    if (error) {
        displayError(error);
    }
    else {
        // The card action has been handled.
        // The PaymentIntent can be confirmed again on the server.
        fetchPay(JSON.stringify({ payment_intent_id: result.paymentIntent.id }))
            .then((confirmResult) => confirmResult.json())
            .then(handleServerResponse)
            .catch((fetchPayError) =>
                displayError(fetchErrorText + ' ' + fetchPayError)
            );
    }
}

function stripePaymentMethodHandler(result) {
    const error = result.error;

    // Show error in payment form.
    if (error) {
        displayError(error);
    }
    else {
        // Otherwise send paymentMethod.id to the server.
        fetchPay(JSON.stringify({ payment_method_id: result.paymentMethod.id }))
            .then((fetchPayResult) => {
                // Handle server response.
                fetchPayResult.json()
                    .then((json) => {
                        handleServerResponse(json);
                    })
                    .catch((fetchPayError) =>
                        displayError(fetchErrorText + ' ' + fetchPayError)
                    );
            });
    }
}

function registerElements() {
    // Displaying card input error.
    card.on('change', (event) => {
        const eventError = event.error;
        if (eventError) {
            displayError(eventError);
        }
        else {
            errorContainer.textContent = '';
        }
    });

    form.addEventListener('submit', (event) => {
        // We don't want to let default form submission happen here, which would refresh the page.
        event.preventDefault();

        disableInputs();

        // eslint-disable-next-line
        stripe.createPaymentMethod({
            type: 'card',
            card: card,
            billing_details: {
                // Include any additional collected billing details.
            },
        }).then(stripePaymentMethodHandler);
    });
}

if (placeOfCard) {
    card.mount(placeOfCard);

    // Refreshing form elements with the card input.
    formElements = form.elements;
    registerElements([card]);
}
