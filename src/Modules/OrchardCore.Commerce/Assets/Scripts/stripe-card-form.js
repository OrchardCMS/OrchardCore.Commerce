// Adding credit card element with Stripe API.
const stripeElements = stripe.elements();

const card = stripeElements.create('card', {
    style: {
        base: {
            fontWeight: '500',
            fontSize: '20px',
        },
    },
});

const placeOfCard = document.querySelector('#card-payment-form_card');


function registerElements(elements) {
    const form = document.querySelector('.card-payment-form');

    // Displaying card input error.
    card.on('change', (event) => {
        const displayError = document.querySelector('.error-message');
        if (event.error) {
            displayError.textContent = event.error.message;
        }
        else {
            displayError.textContent = '';
        }
    });

    // We need to generate a Stripe token before submitting the form.
    form.addEventListener('submit', (e) => {
        e.preventDefault();

        //const formId = '#card-payment-form';

        // Gather additional customer data we may have collected in our form. To do: Pass shipping data when shipping is
        // implemented.
        //const name = form.querySelector(formId + '_name');
        //const address1 = form.querySelector(formId + '_address');
        //const city = form.querySelector(formId + '_city');
        //const state = form.querySelector(formId + '_state');
        //const zip = form.querySelector(formId + '_zip');
        //const additionalData = {
        //    name: name ? name.value : undefined,
        //    address_line1: address1 ? address1.value : undefined,
        //    address_city: city ? city.value : undefined,
        //    address_state: state ? state.value : undefined,
        //    address_zip: zip ? zip.value : undefined,
        //};

        // Later if we wish, we can add more payment methods.
        stripe.createPaymentMethod({
            type: 'card',
            card: card,
            billing_details: {
                // Include any additional collected billing details.
            },
        }).then((result) => {
            document.querySelector('#paymentMethod').value = result.paymentMethod.id;
            document.querySelector('#card-payment-form').submit();
        });
    });
}

if (placeOfCard != null) {
    card.mount(placeOfCard);
    registerElements([card]);
}
