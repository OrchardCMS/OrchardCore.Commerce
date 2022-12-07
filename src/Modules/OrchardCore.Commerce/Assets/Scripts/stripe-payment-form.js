window.stripePaymentForm = function stripePaymentForm(stripe, clientSecret, baseUrl, antiForgeryToken, urlPrefix, fetchErrorText, missingText) {
    const allErrorContainers = [ document.querySelector('.message-error') ];
    const form = document.querySelector('.payment-form');
    const submitButton = form.querySelector('.pay-button');
    const payText = form.querySelector('.pay-text');
    const paymentProcessingContainer = form.querySelector('.payment-processing-container');
    const selectTagName = 'SELECT';
    let formElements = Array.from(form.elements);

    const appearance = {
        theme: 'stripe',
        variables: {
            colorText: '#32325d',
            fontFamily: '"Helvetica Neue", Helvetica, sans-serif',
        },
    };

    const stripeElements = stripe.elements({
        clientSecret: clientSecret,
    });

    const payment = stripeElements.create('payment', {
        fields: {
            billingDetails: 'never',
        }
    });

    const placeOfPayment = document.querySelector('#payment-form_payment');

    function toggleInputs(enable) {
        formElements.forEach((element) => {
            if (element.tagName === selectTagName) {
                element.disabled = !enable;
            }

            element.readOnly = !enable;
        });

        payment.update({ disabled: !enable });

        submitButton.disabled = !enable;

        paymentProcessingContainer.hidden = enable;
        payText.hidden = !enable;
    }

    function displayError(errors, container = allErrorContainers[0]) {
        allErrorContainers.forEach((element) => element.hidden = true);
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
        else if (response.success) {
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
        document.getElementById('StripePaymentPart_PaymentMethodId_Text').value = result.paymentIntent.paymentMethod.id;

        // The payment action has been handled.
        // The PaymentIntent can be confirmed again on the server.
        return fetchPay({ paymentId: result.paymentIntent.id });
    };

    function getText(element) { return element?.textContent.trim(); }

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

            let result;
            try {
                const emptyRequiredFields = Array.from(form.querySelectorAll('input'))
                    .filter((element) => element.required && !element.hidden)
                    .filter((element) => !element.value?.match(/\S+/));

                if (emptyRequiredFields.length) {
                    throw emptyRequiredFields
                        .map((element) => document.querySelector(`label[for="${element.id}"]`))
                        .filter(getText)
                        .filter((label) => !label.closest('.address')?.hidden)
                        .map((label) => {
                            var title = getText(label.closest('.address')?.querySelector('.address__title'));
                            let name = title ? `${title} ${getText(label)}` : getText(label);

                            return missingText.replace('%LABEL%', name);
                        });
                }

                const validationJson = await fetchPost('checkout/validate', { body: new FormData(form) });
                if (validationJson?.errors?.length) throw validationJson.errors;

                stripe.confirmPayment({
                    elements: stripeElements,
                    confirmParams: {
                        // return_url: `${baseUrl}/${urlPrefix}/success/${proposedOrderContentItemId}`,
                        return_url: `${baseUrl}/checkout`,
                        payment_method_data: {
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
                                }
                            }
                        },
                    },
                    redirect: "if_required",
                })
                .then(async function(result) {
                    await handleStripeJsResult(result);
                });
            }
            catch (error) {
                result = { error };
                displayError(result.error);
            }
        });
    }

    async function checkStatus() {
        const urlParams = new URLSearchParams(window.location.search);
        const paymentIntentId = urlParams.get(
            "payment_intent"
        );

        const clientSecret = urlParams.get(
            "payment_intent_client_secret"
        );

        if (!clientSecret || !paymentIntentId) {
            return;
        }

        const { paymentIntent } = await stripe.retrievePaymentIntent(clientSecret);

        document.getElementById('StripePaymentPart_PaymentIntentId_Text').value = paymentIntentId;
        document.getElementById('StripePaymentPart_PaymentMethodId_Text').value = paymentIntent.payment_method;

        // The PaymentIntent can be confirmed again on the server.
        return fetchPay({ paymentId: paymentIntentId });

        // const { paymentIntent } = await stripe.retrievePaymentIntent(clientSecret);
        //
        // switch (paymentIntent.status) {
        //     case "succeeded":
        //         showMessage("Payment succeeded!");
        //         break;
        //     case "processing":
        //         showMessage("Your payment is processing.");
        //         break;
        //     case "requires_payment_method":
        //         showMessage("Your payment was not successful, please try again.");
        //         break;
        //     default:
        //         showMessage("Something went wrong.");
        //         break;
        // }
    }


    if (placeOfPayment) {
        payment.mount(placeOfPayment);

        // Refreshing form elements with the payment input.
        formElements = Array.from(form.elements);
        registerElements();
    }

    checkStatus();
};
