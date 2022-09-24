window.initializeToggleSecondAddress = function(
    checkbox,
    firstAddressRow,
    secondAddressRow) {

    function copyValue(elementName) {
        const selector = '.address__' + elementName;
        const target = secondAddressRow.querySelector(selector);
        target.value = firstAddressRow.querySelector(selector).value;
        target.change();
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
    }

    checkbox.addEventListener('change', onCheckboxChange);
    onCheckboxChange();
};
