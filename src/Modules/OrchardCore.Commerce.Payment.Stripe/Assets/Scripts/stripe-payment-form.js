window.stripePaymentForm = function stripePaymentForm(
    stripe,
    clientSecret,
    baseUrl,
    antiForgeryToken,
    urlPrefix,
    errorText,
    missingText,
    updatePaymentIntentUrl) {
    const phone = document.getElementById('OrderPart_Phone_Text');
    const email = document.getElementById('OrderPart_Email_Text');

    const shippingAddressName = document.getElementById('OrderPart_ShippingAddress_Address_Name');
    const shippingAddressCity = document.getElementById('OrderPart_ShippingAddress_Address_City');
    const shippingAddressCountry = document.getElementById('OrderPart_ShippingAddress_Address_Region');
    const shippingAddressStreetAddress1 = document.getElementById('OrderPart_ShippingAddress_Address_StreetAddress1');
    const shippingAddressStreetAddress2 = document.getElementById('OrderPart_ShippingAddress_Address_StreetAddress2');
    const shippingAddressPostalCode = document.getElementById('OrderPart_ShippingAddress_Address_PostalCode');
    const shippingAddressState = document.getElementById('OrderPart_ShippingAddress_Address_Province');

    const billingAddressName = document.getElementById('OrderPart_BillingAddress_Address_Name');
    const billingAddressCity = document.getElementById('OrderPart_BillingAddress_Address_City');
    const billingAddressCountry = document.getElementById('OrderPart_BillingAddress_Address_Region');
    const billingAddressStreetAddress1 = document.getElementById('OrderPart_BillingAddress_Address_StreetAddress1');
    const billingAddressStreetAddress2 = document.getElementById('OrderPart_BillingAddress_Address_StreetAddress2');
    const billingAddressPostalCode = document.getElementById('OrderPart_BillingAddress_Address_PostalCode');
    const billingAddressState = document.getElementById('OrderPart_BillingAddress_Address_Province');

    const allErrorContainers = [document.querySelector('.message-error')];
    const form = document.querySelector('.payment-form');
    const submitButton = form.querySelector('.pay-button-stripe');
    const payText = submitButton.querySelector('.pay-text');
    const paymentProcessingContainer = submitButton.querySelector('.payment-processing-container');
    const stripeElements = stripe.elements({
        clientSecret,
    });
    const payment = stripeElements.create('payment', {
        fields: {
            billingDetails: 'never',
        },
    });
    const placeOfPayment = document.querySelector('#payment-form_payment');

    let formElements = Array.from(form.elements);

    function toggleInputs(enable) {
        formElements.forEach((element) => {
            element.readOnly = !enable;
        });

        payment.update({ disabled: !enable });

        submitButton.disabled = !enable;

        paymentProcessingContainer.hidden = enable;
        payText.hidden = !enable;
    }

    function displayError(errors, container = allErrorContainers[0]) {
        allErrorContainers.forEach((element) => { element.hidden = true; });
        if (!errors || errors.length === 0) return;

        const err = Array.isArray(errors) ? errors.filter((error) => error) : [errors];

        container.innerHTML = '<ul></ul>';
        const ul = container.querySelector('ul');
        for (let i = 0; i < err.length; i++) {
            const li = document.createElement('li');
            li.textContent = Object.prototype.hasOwnProperty.call(err[i], 'message') ? err[i].message : err[i];
            ul.appendChild(li);
        }

        toggleInputs(true);

        container.hidden = false;
        container.scrollIntoView({ block: 'center' });
    }

    function fetchPost(path, options) {
        return fetch(`${urlPrefix}/${path}`, { method: 'POST', ...options })
            .then((response) => response.json());
    }

    function getText(element) {
        return element?.textContent.trim();
    }

    function registerElements() {
        // Displaying payment input error.
        const stripeFieldError = document.querySelector('.stripe-field-error');
        allErrorContainers.push(stripeFieldError);
        payment.on('change', (event) => {
            displayError(event?.error, stripeFieldError);
        });

        submitButton.addEventListener('click', async (event) => {
            // We don't want to let default form submission happen here, which would refresh the page.
            event.preventDefault();
            toggleInputs(false);

            const { paymentIntent } = await stripe.retrievePaymentIntent(clientSecret);

            if (paymentIntent.status !== 'succeeded' && paymentIntent.last_payment_error) {
                displayError(paymentIntent.last_payment_error);
                return;
            }

            await fetch(updatePaymentIntentUrl.replace('PAYMENT_INTENT', paymentIntent.id));

            let result;
            try {
                const emptyRequiredFields = Array.from(form.querySelectorAll('input'))
                    .filter((element) => element.required && !element.hidden)
                    .filter((element) => !element.value?.match(/\S+/));

                if (emptyRequiredFields.length) {
                    toggleInputs(true);
                    throw emptyRequiredFields
                        .map((element) => document.querySelector(`label[for="${element.id}"]`))
                        .filter(getText)
                        .filter((label) => !label.closest('.address')?.hidden)
                        .map((label) => {
                            const title = getText(label.closest('.address')?.querySelector('.address__title'));
                            const name = title ? `${title} ${getText(label)}` : getText(label);

                            return missingText.replace('%LABEL%', name);
                        });
                }

                const validationJson = await fetchPost('checkout/validate/Stripe', { body: new FormData(form) });
                if (validationJson?.errors?.length) {
                    toggleInputs(true);
                    throw validationJson.errors;
                }

                result = await stripe.confirmPayment({
                    elements: stripeElements,
                    confirmParams: {
                        return_url: `${baseUrl}${urlPrefix}/checkout/middleware/Stripe`,
                        payment_method_data: {
                            billing_details: {
                                email: email.value,
                                name: billingAddressName.value,
                                phone: phone.value,
                                address: {
                                    city: billingAddressCity.value,
                                    country: billingAddressCountry.value,
                                    line1: billingAddressStreetAddress1.value,
                                    line2: billingAddressStreetAddress2.value,
                                    postal_code: billingAddressPostalCode.value,
                                    state: billingAddressState.value,
                                },
                            },
                        },
                        shipping: {
                            name: shippingAddressName.value,
                            phone: phone.value,
                            address: {
                                city: shippingAddressCity.value,
                                country: shippingAddressCountry.value,
                                line1: shippingAddressStreetAddress1.value,
                                line2: shippingAddressStreetAddress2.value,
                                postal_code: shippingAddressPostalCode.value,
                                state: shippingAddressState.value,
                            },
                        },
                    },
                });

                displayError(result.error);
            }
            catch (error) {
                result = { error };
                displayError(result.error);
            }
        });
    }

    function registerPriceUpdater() {
        let debounce = false;
        Array.from(document.querySelectorAll('*[id^="OrderPart_ShippingAddress_"], *[id^="OrderPart_BillingAddress_"]'))
            .forEach((element) => element.addEventListener('change', () => {
                if (debounce) return;

                const payButtonValue = document.querySelector('.pay-button-value');
                if (!payButtonValue) return;

                debounce = true;
                submitButton.disabled = true;

                setTimeout(async () => {
                    const priceJson = await fetchPost('checkout/price', { body: new FormData(form) });
                    debounce = false;
                    submitButton.disabled = false;

                    // This is not essential if it fails so we intentionally don't catch it. This way if there is an error it can still be seen in the
                    // browser log during development or UI testing.
                    if ('error' in priceJson) throw priceJson;

                    payButtonValue.setAttribute('data-value', priceJson.value);
                    payButtonValue.setAttribute('data-currency', priceJson.currency);
                    payButtonValue.textContent = priceJson.text;
                }, 50); // Prevent multiple requests when several fields are updated at once.
            }));
    }

    if (placeOfPayment) {
        payment.mount(placeOfPayment);

        // Refreshing form elements with the payment input.
        formElements = Array.from(form.elements);
        registerElements();

        registerPriceUpdater();
    }
};
