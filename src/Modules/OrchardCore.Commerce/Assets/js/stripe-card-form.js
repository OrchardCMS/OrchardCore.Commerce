var stripe = Stripe
    ('pk_test_51H59owJmQoVhz82aWAoi9M5s8PC6sSAqFI7KfAD2NRKun5riDIOM0dvu2caM25a5f5JbYLMc5Umxw8Dl7dBIDNwM00yVbSX8uS')

function registerElements(elements, className) {
    var formClass = '.' + className;
    var example = document.querySelector(formClass);

    var form = example.querySelector('form');
    var resetButton = example.querySelector('a.reset');
    var error = form.querySelector('.error');
    var errorMessage = error.querySelector('.message');

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

    function triggerBrowserValidation() {
        // The only way to trigger HTML5 form validation UI is to fake a user submit
        // event.
        var submit = document.createElement('input');
        submit.type = 'submit';
        submit.style.display = 'none';
        form.appendChild(submit);
        submit.click();
        submit.remove();
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

        // Show a loading screen...
        example.classList.add('submitting');

        // Disable all inputs.
        disableInputs();

        // Gather additional customer data we may have collected in our form.
        var name = form.querySelector('#' + className + '_name');
        var email = form.querySelector('#' + className + '_email');
        var address1 = form.querySelector('#' + className + '_address');
        var city = form.querySelector('#' + className + '_city');
        var state = form.querySelector('#' + className + '_state');
        var zip = form.querySelector('#' + className + '_zip');
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
                console.log("Received token = " + result.token.id);

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

    resetButton.addEventListener('click', function (e) {
        e.preventDefault();
        // Resetting the form (instead of setting the value to `''` for each input)
        // helps us clear webkit autofill styles.
        form.reset();

        // Clear each Element.
        elements.forEach(function (element) {
            element.clear();
        });

        // Reset error state as well.
        error.classList.remove('visible');

        // Resetting the form does not un-disable inputs, so we need to do it separately:
        enableInputs();
        example.classList.remove('submitted');
    });
}

var elements = stripe.elements({
    fonts: [
        {
            cssSrc: 'https://fonts.googleapis.com/css?family=Roboto',
        },
    ],
    locale: 'auto'
});

var card = elements.create('card', {
    iconStyle: 'solid',
    //style: {
    //    base: {
    //        iconColor: '#c4f0ff',
    //        color: '#fff',
    //        fontWeight: 500,
    //        fontFamily: 'Roboto, Open Sans, Segoe UI, sans-serif',
    //        fontSize: '16px',
    //        fontSmoothing: 'antialiased',

    //        ':-webkit-autofill': {
    //            color: '#fce883',
    //        },
    //        '::placeholder': {
    //            color: '#87BBFD',
    //        },
    //    },
    //    invalid: {
    //        iconColor: '#FFC7EE',
    //        color: '#FFC7EE',
    //    },
    //},
});

card.mount('#card-payment-form_card');

registerElements([card], 'card-payment-form');
