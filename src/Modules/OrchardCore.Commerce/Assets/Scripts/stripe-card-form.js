// Adding credit card element with Stripe API.
const stripeElements = stripe.elements();
const errorContainer = document.querySelector('.error-message');
const form = document.querySelector('.card-payment-form');
const submitButton = form.querySelector('button[type="submit"]');

const card = stripeElements.create('card', {
    style: {
        base: {
            fontWeight: '500',
            fontSize: '20px',
        },
    },
});

const placeOfCard = document.querySelector('#card-payment-form_card');

if (placeOfCard) {
    card.mount(placeOfCard);
    registerElements([card]);
}

const formElements = form.elements;

function handleStripeJsResult(result) {
    const error = result.error;

    // Show error in payment form.
    if (error) {
        displayError(error);
    }
    else {
        // The card action has been handled.
        // The PaymentIntent can be confirmed again on the server.
        fetch('/pay', {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ payment_intent_id: result.paymentIntent.id })
        }).then(function (confirmResult) {
            return confirmResult.json();
        }).then(handleServerResponse);
    }
}

function handleServerResponse(response) {
    const error = response.error;

    // Show error in payment form.
    if (error) {
        displayError(error);
    }
    else if (response.requires_action) {
        // Use Stripe.js to handle required card action (like 3DS authentication).
        stripe.handleCardAction(
            response.payment_intent_client_secret
        ).then(handleStripeJsResult);
    }
    else {
        // Show success message.
        window.location.href = '/success';
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
        fetch('/pay', {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                payment_method_id: result.paymentMethod.id,
            })
        }).then(function (result) {
            // Handle server response.
            result.json().then(function (json) {
                handleServerResponse(json);
            })
        });
    }
}

function registerElements(elements) {
    // Displaying card input error.
    card.on('change', (event) => {
        const eventError = event.error
        if (eventError) {
            displayError(eventError);
        }
        else {
            errorContainer.textContent = '';
        }
    });

    form.addEventListener('submit', (e) => {
        // We don't want to let default form submission happen here,
        // which would refresh the page.
        event.preventDefault();

        disableInputs();

        stripe.createPaymentMethod({
            type: 'card',
            card: card,
            billing_details: {
                // Include any additional collected billing details.
            },
        }).then(stripePaymentMethodHandler);
    });
}

function displayError(error) {
    if (error.hasOwnProperty('message')) {
        errorContainer.textContent = error.message;
    }
    else {
        errorContainer.textContent = error;
    }

    // Enable inputs.
    for (var i = 0, length = formElements.length; i < length; ++i) {
        formElements[i].readOnly = false;
    }
    card.update({ disabled: false });

    submitButton.disabled = false;
}

function disableInputs() {
    for (var i = 0, length = formElements.length; i < length; ++i) {
        formElements[i].readOnly = true;
    }

    card.update({ disabled: true });

    submitButton.disabled = true;
}
