(function setupStripePaymentForm(Stripe) {
    function toString(data, key, defaultValue) {
        if (!data[key]) return defaultValue;
        const value = `${data[key]}`.trim();
        return value.length > 0 ? value : defaultValue;
    }

    function stripePaymentForm(data) {
        const stripe = Stripe(data.publishableKey, data.stripeAccountId);
        const validateUrl = toString(data, 'validateUrl', 'checkout/validate/stripe');
        const paramsUrl = toString(data, 'paramsUrl', 'stripe/params');
        const priceUrl = toString(data, 'priceUrl', 'checkout/price');
        const errorContainerSelector = toString(data, 'errorContainerSelector', '.message-error');
        const stripeFieldErrorSelector = toString(data, 'stripeFieldErrorSelector', '.stripe-field-error');
        const paymentFormSelector = toString(data, 'paymentFormSelector', '.payment-form');
        const payButtonSelector = toString(data, 'payButtonSelector', '.pay-button-stripe');
        const payTextSelector = toString(data, 'payTextSelector', '.pay-text');
        const paymentProcessingContainerSelector = toString(
            data,
            'paymentProcessingContainerSelector',
            '.payment-processing-container');
        const placeOfPaymentSelector = toString(data, 'placeOfPaymentSelector', '#payment-form_payment');
        const payButtonValueSelector = toString(data, 'payButtonValueSelector', '.pay-button-value');
        const addressesSelector = toString(
            data,
            'addressesSelector',
            '*[id^="OrderPart_ShippingAddress_"], *[id^="OrderPart_BillingAddress_"]');
        const addressSelector = toString(data, 'addressSelector', '.address');
        const addressTitleSelector = toString(data, 'addressTitleSelector', '.address__title');

        const allErrorContainers = [document.querySelector(errorContainerSelector)];
        const form = document.querySelector(paymentFormSelector);
        const submitButton = form.querySelector(payButtonSelector);
        const payText = submitButton.querySelector(payTextSelector);
        const paymentProcessingContainer = submitButton.querySelector(paymentProcessingContainerSelector);
        const stripeElements = stripe.elements({ clientSecret: data.clientSecret });
        const payment = stripeElements.create('payment', { fields: { billingDetails: 'never' } });
        const placeOfPayment = document.querySelector(placeOfPaymentSelector);

        let formElements = Array.from(form.elements);

        function toggleInputs(enable) {
            formElements.forEach((element) => {
                element.readOnly = !enable;
            });

            payment.update({ readOnly: !enable });
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
            err.forEach((error) => {
                const li = document.createElement('li');
                li.textContent = error.message || error;
                ul.appendChild(li);
            });

            toggleInputs(true);
            container.hidden = false;
            container.scrollIntoView({ block: 'center' });
        }

        function fetchPost(path) {
            return fetch(`${data.urlPrefix}/${path}`, { method: 'POST', body: new FormData(form) })
                .then((response) => response.json());
        }

        function getText(element) {
            return element?.textContent.trim();
        }

        function registerElements() {
            // Displaying payment input error.
            const stripeFieldError = document.querySelector(stripeFieldErrorSelector);
            allErrorContainers.push(stripeFieldError);
            payment.on('change', (event) => {
                displayError(event?.error, stripeFieldError);
            });

            submitButton.addEventListener('click', async (event) => {
                // We don't want to let default form submission happen here, which would refresh the page.
                event.preventDefault();
                toggleInputs(false);

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
                            .filter((label) => !label.closest(addressSelector)?.hidden)
                            .map((label) => {
                                const title = getText(label.closest(addressSelector)?.querySelector(addressTitleSelector));
                                const name = title ? `${title} ${getText(label)}` : getText(label);
                                return data.missingText.replace('%LABEL%', name);
                            });
                    }

                    const validationJson = await fetchPost(`${validateUrl}/${data.paymentIntentId}`);
                    if (validationJson?.errors?.length) {
                        toggleInputs(true);
                        throw validationJson.errors;
                    }

                    const confirmPaymentOptions = {
                        elements: stripeElements,
                        confirmParams: await fetchPost(paramsUrl),
                    };
                    result = await stripe.confirmPayment(confirmPaymentOptions);

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
            Array.from(document.querySelectorAll(addressesSelector))
                .forEach((element) => element.addEventListener('change', () => {
                    if (debounce) return;

                    const payButtonValue = document.querySelector(payButtonValueSelector);
                    if (!payButtonValue) return;

                    debounce = true;
                    submitButton.disabled = true;

                    setTimeout(async () => {
                        const priceJson = await fetchPost(priceUrl);
                        debounce = false;
                        submitButton.disabled = false;

                        // This is not essential if it fails so we intentionally don't catch it. This way if there is an
                        // error it can still be seen in the browser log during development or UI testing.
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
    }

    window.stripePaymentForm = stripePaymentForm;
})(window.Stripe);
