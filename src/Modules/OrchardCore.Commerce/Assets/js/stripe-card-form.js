var stripe = Stripe
    ('pk_test_51H59owJmQoVhz82aWAoi9M5s8PC6sSAqFI7KfAD2NRKun5riDIOM0dvu2caM25a5f5JbYLMc5Umxw8Dl7dBIDNwM00yVbSX8uS')

function registerElements(elements) {
    var form = document.querySelector('.card-payment-form');
    var error = form.querySelector('.error');
    var errorMessage = error.querySelector('.error-message');

    function enableInputs() {
        Array.prototype.forEach.call(
            form.querySelectorAll(
                "input[type='text'], input[type='email'], input[type='tel']"
            ),
            function (input) {
                input.removeAttribute('disabled');
            }
        );
    }

    function disableInputs() {
        Array.prototype.forEach.call(
            form.querySelectorAll(
                "input[type='text'], input[type='email'], input[type='tel']"
            ),
            function (input) {
                input.setAttribute('disabled', 'true');
            }
        );
    }

    // Listen for errors from each Element, and show error messages in the UI.
    var savedErrors = {};
    elements.forEach(function (element, idx) {
        element.on('change', function (event) {
            if (event.error) {
                error.classList.add('visible');
                savedErrors[idx] = event.error.message;
                errorMessage.innerText = event.error.message;
            } else {
                savedErrors[idx] = null;

                // Loop over the saved errors and find the first one, if any.
                var nextError = Object.keys(savedErrors)
                    .sort()
                    .reduce(function (maybeFoundError, key) {
                        return maybeFoundError || savedErrors[key];
                    }, null);

                if (nextError) {
                    // Now that they've fixed the current error, show another one.
                    errorMessage.innerText = nextError;
                } else {
                    // The user fixed the last error; no more errors.
                    error.classList.remove('visible');
                }
            }
        });
    });

    // Listen on the form's 'submit' handler...
    form.addEventListener('submit', function (e) {
        e.preventDefault();

        // Trigger HTML5 validation UI on the form if any of the inputs fail
        // validation.
        var plainInputsValid = true;
        Array.prototype.forEach.call(form.querySelectorAll('input'), function (
            input
        ) {
            if (input.checkValidity && !input.checkValidity()) {
                plainInputsValid = false;
                return;
            }
        });
        if (!plainInputsValid) {
            triggerBrowserValidation();
            return;
        }

        disableInputs();

        var formId = '#card-payment-form';

        // Gather additional customer data we may have collected in our form.
        var name = form.querySelector(formId + '_name');
        var email = form.querySelector(formId + '_email');
        var address1 = form.querySelector(formId + '_address');
        var city = form.querySelector(formId + '_city');
        var state = form.querySelector(formId + '_state');
        var zip = form.querySelector(formId + '_zip');
        var additionalData = {
            name: name ? name.value : undefined,
            address_line1: address1 ? address1.value : undefined,
            address_city: city ? city.value : undefined,
            address_state: state ? state.value : undefined,
            address_zip: zip ? zip.value : undefined,
        };

        // Use Stripe.js to create a token. We only need to pass in one Element
        // from the Element group in order to create a token. We can also pass
        // in the additional customer data we collected in our form.
        stripe.createToken(elements[0], additionalData).then(function (result) {
            if (result.token) {
                // You can test if the token was created:
                // console.log("Received token = " + result.token.id);

                // Insert the email and token into the form so it gets submitted to the server
                document.querySelector('#hiddenEmail').value = email.value;
                document.querySelector('#hiddenToken').value = result.token.id;
                // and submit
                document.querySelector('#card-payment-form').submit();

            } else {
                // Otherwise, un-disable inputs.
                enableInputs();
            }
        });
    });
}

// Adding credit card element with Stripe API.

var elements = stripe.elements();

var card = elements.create('card', {
    style: {
        base: {
            fontWeight: '500',
            fontSize: '20px',
        },
    },
});

card.mount('#card-payment-form_card');

registerElements([card]);
