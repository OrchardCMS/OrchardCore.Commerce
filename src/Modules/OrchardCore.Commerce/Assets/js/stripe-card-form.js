var stripe = Stripe
    ('pk_test_51H59owJmQoVhz82aWAoi9M5s8PC6sSAqFI7KfAD2NRKun5riDIOM0dvu2caM25a5f5JbYLMc5Umxw8Dl7dBIDNwM00yVbSX8uS')

function registerElements(elements) {
    var form = document.querySelector('.card-payment-form');
    var error = form.querySelector('.error');
    var errorMessage = error.querySelector('.error-message');

    // We need to generate a Stripe token before submitting the form.
    form.addEventListener('submit', function (e) {
        e.preventDefault();

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

                // Insert the token into the form so it gets submitted to the server
                document.querySelector('#hiddenToken').value = result.token.id;

                document.querySelector('#card-payment-form').submit();

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
