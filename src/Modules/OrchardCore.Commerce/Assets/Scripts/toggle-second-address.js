window.initializeToggleSecondAddress = function (
    checkbox,
    firstAddressRow,
    secondAddressRow) {

    function copyValue(elementName) {
        const selector = '.address__' + elementName;
        const target = secondAddressRow.querySelector(selector);
        const source = firstAddressRow.querySelector(selector);
        if (!target || !source) return;

        target.value = source.value;
        target.checked = source.checked;
        target.dispatchEvent(new Event('change'));
    }

    function onCheckboxChange() {
        secondAddressRow.hidden = checkbox.checked;
        if (!checkbox.checked) return;

        copyValue('name');
        copyValue('department');
        copyValue('company');
        copyValue('street_first');
        copyValue('street_second');
        copyValue('city');
        copyValue('postalCode');
        copyValue('region');
        copyValue('province');
        copyValue('toBeSaved');
    }

    checkbox.addEventListener('change', onCheckboxChange);
    onCheckboxChange();

    Array.from(firstAddressRow.querySelectorAll('input, select'))
        .forEach((input) => input.addEventListener('change', onCheckboxChange));
};

(function autoInitializeToggleSecondAddress() {
    if (document.getElementById('UserAddressesPart_BillingAndShippingAddressesMatch_Value')) {
        initializeToggleSecondAddress(
            document.getElementById('UserAddressesPart_BillingAndShippingAddressesMatch_Value'),
            document.querySelector('.address_billing-address'),
            document.querySelector('.address_shipping-address'));
    }

    if (document.getElementById('OrderPart_BillingAndShippingAddressesMatch_Value')) {
        initializeToggleSecondAddress(
            document.getElementById('OrderPart_BillingAndShippingAddressesMatch_Value'),
            document.querySelector('.address_billing-address'),
            document.querySelector('.address_shipping-address'));
    }
})();

