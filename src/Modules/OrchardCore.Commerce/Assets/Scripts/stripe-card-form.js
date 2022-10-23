window.stripeCardForm = function stripeCardForm(stripe, antiForgeryToken, urlPrefix, fetchErrorText, missingText) {
    const stripeElements = stripe.elements();
    const errorContainer = document.querySelector('.message-error');
    const form = document.querySelector('.card-payment-form');
    const submitButton = form.querySelector('.pay-button');
    const payText = form.querySelector('.pay-text');
    const paymentProcessingContainer = form.querySelector('.payment-processing-container');
    const selectTagName = 'SELECT';
    let formElements = Array.from(form.elements);

    const card = stripeElements.create('card', {
        style: {
            base: {
                fontWeight: '500',
                fontSize: '20px',
            },
        },
    });

    const placeOfCard = document.querySelector('#card-payment-form_card');

    function toggleInputs(enable) {
        formElements.forEach((element) => {
            if (element.tagName === selectTagName) {
                element.disabled = !enable;
            }

            element.readOnly = !enable;
        });
    }

    function disableInputsAndShowSpinner() {
        toggleInputs(false);
        card.update({ disabled: true });

        submitButton.disabled = true;

        paymentProcessingContainer.hidden = false;
        payText.hidden = true;
    }

    function displayError(errors, container = errorContainer) {
        if (!errors || errors.length === 0) {
            container.hidden = true;
            return;
        }

        const err = Array.isArray(errors) ? errors.filter((error) => error) : [errors];

        container.innerHTML = '<ul></ul>';
        const ul = container.querySelector('ul');
        for (let i = 0; i < err.length; i++) {
            const li = document.createElement('li');
            li.textContent = Object.prototype.hasOwnProperty.call(err[i], 'message') ? err[i].message : err[i];
            ul.appendChild(li);
        }

        toggleInputs(true);
        card.update({ disabled: false });
        submitButton.disabled = false;

        paymentProcessingContainer.hidden = true;
        payText.hidden = false;
        container.hidden = false;
        container.scrollIntoView({ block: 'center' });
    }

    function fetchPost(path, options) {
        return fetch(`${urlPrefix}/${path}`, { method: 'POST', ...options })
            .then((response) => response.json());
    }

    let handleServerResponse;
    let handleStripeJsResult;

    function fetchPay(data) {
        // eslint-disable-next-line dot-notation -- That would throw "no-underscore-dangle". This looks better anyway.
        data['__RequestVerificationToken'] = antiForgeryToken;

        return fetchPost('pay', {
            headers: {
                Accept: 'application/json',
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: Object
                .entries(data)
                .map((pair) => pair.map(encodeURIComponent).join('='))
                .join('&'),
        })
            .then(handleServerResponse)
            .catch((fetchPayError) => displayError(fetchErrorText + ' ' + fetchPayError));
    }

    handleServerResponse = function (response) {
        const error = response.error;

        // Show error in payment form.
        if (error) {
            displayError(error);
            return Promise.reject(error);
        }

        if (response.requires_action) {
            // Use Stripe.js to handle required card action (like 3DS authentication).
            stripe.handleCardAction(response.payment_intent_client_secret)
                .then(handleStripeJsResult);
        }
        else {
            // Show success message.
            form.action = `${urlPrefix}/success/${response.orderContentItemId}`;
            form.method = 'POST';
            form.submit();
        }

        return Promise.resolve();
    };

    handleStripeJsResult = function (result) {
        // Show error in payment form.
        if (result.error) return displayError(result.error);

        document.getElementById('StripePaymentPart_PaymentIntentId_Text').value = result.paymentIntent.id;

        // The card action has been handled.
        // The PaymentIntent can be confirmed again on the server.
        return fetchPay({ paymentIntentId: result.paymentIntent.id });
    };

    function stripePaymentMethodHandler(result) {
        // Show error in payment form.
        if (result.error) return displayError(result.error);

        document.getElementById('StripePaymentPart_PaymentMethodId_Text').value = result.paymentMethod.id;

        // Otherwise send paymentMethod.id to the server.
        return fetchPay({ paymentMethodId: result.paymentMethod.id });
    }

    function registerElements() {
        // Displaying card input error.
        const stripeFieldError = document.querySelector('.stripe-field-error');
        card.on('change', (event) => {
            displayError(false);
            displayError(event?.error, stripeFieldError);
        });

        submitButton.addEventListener('click', async (event) => {
            // We don't want to let default form submission happen here, which would refresh the page.
            event.preventDefault();
            disableInputsAndShowSpinner();

            let result;
            try {
                const emptyRequiredFields = Array.from(form.querySelectorAll('input'))
                    .filter((element) => element.required && !element.hidden)
                    .filter((element) => !element.value?.match(/\S+/));

                if (emptyRequiredFields.length) {
                    throw emptyRequiredFields
                        .map((element) => document.querySelector(`label[for="${element.id}"]`)?.textContent)
                        .filter((label) => label)
                        .map((label) => missingText.replace('%LABEL%', label));
                }

                const validationJson = await fetchPost('checkout/validate', { body: new FormData(form) });
                if (validationJson?.errors?.length) throw validationJson.errors;

                result = await stripe.createPaymentMethod({
                    type: 'card',
                    card: card,
                    billing_details: {
                        email: document.getElementById('OrderPart_Email_Text').value,
                        name: document.getElementById('OrderPart_BillingAddress_Address_Name').value,
                        phone: document.getElementById('OrderPart_Phone_Text').value,
                        address: {
                            city: document.getElementById('OrderPart_BillingAddress_Address_City').value,
                            country: document.getElementById('OrderPart_BillingAddress_Address_Region').value,
                            line1: document.getElementById('OrderPart_BillingAddress_Address_StreetAddress1').value,
                            line2: document.getElementById('OrderPart_BillingAddress_Address_StreetAddress2').value,
                            postal_code: document.getElementById('OrderPart_BillingAddress_Address_PostalCode').value,
                            state: document.getElementById('OrderPart_BillingAddress_Address_Province').value,
                        },
                    },
                });
            }
            catch (error) {
                result = { error };
                console.log(result);
            }

            await stripePaymentMethodHandler(result);
        });
    }

    if (placeOfCard) {
        card.mount(placeOfCard);

        // Refreshing form elements with the card input.
        formElements = Array.from(form.elements);
        registerElements();
    }
};
