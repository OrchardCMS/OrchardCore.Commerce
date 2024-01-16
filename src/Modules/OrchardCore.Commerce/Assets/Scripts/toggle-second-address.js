window.initializeToggleSecondAddress = (
    checkbox,
    firstAddressRow,
    secondAddressRow) => {

    function addEvent(items, event, callback) {
        Array.from(items).forEach((item) => item.addEventListener(event, callback));
    }

    function onCheckboxChange() {
        secondAddressRow.hidden = checkbox.checked;
        if (checkbox.checked) {
            Array.from(document.querySelectorAll('.address_billing-address *[name*=".BillingAddress."]'))
                .map((input) => [
                    input,
                    document.getElementsByName(input.name.replace('.BillingAddress.', '.ShippingAddress.'))[0],
                ])
                .filter((pair) => pair[1])
                .forEach((pair) => {
                    pair[1].value = pair[0].value;
                    pair[1].checked = pair[0].checked;
                });
        }
    }

    checkbox.addEventListener('change', onCheckboxChange);
    onCheckboxChange();

    addEvent(firstAddressRow.querySelectorAll('*[name]'), 'change', onCheckboxChange);
    addEvent(document.forms, 'submit', onCheckboxChange);
    setTimeout(onCheckboxChange, 50);
};

(function autoInitializeToggleSecondAddress() {
    Array.from(document.querySelectorAll('[name*="BillingAndShippingAddressesMatch.Value"]'))
        .forEach((checbox) => {
            window.initializeToggleSecondAddress(
                checbox,
                document.querySelector('.address_billing-address'),
                document.querySelector('.address_shipping-address'));
        });
})();
