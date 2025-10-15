document.querySelectorAll('.taxRateSettings').forEach((wrapperElement) => {
    const tableElement = wrapperElement.querySelector('.taxRateSettings__table');
    const tableStyle = wrapperElement.querySelector('.taxRateSettings__table').style;
    const checkbox = wrapperElement.querySelector('.taxRateSettings__hideAddressColumns');

    const rates = JSON.parse(tableElement.getAttribute('data-rates'));
    const newRowJson = tableElement.getAttribute('data-new-row');

    const table = new Vue({
        el: tableElement.querySelector('tbody'),
        data: {
            rates: rates,
        },
    });

    wrapperElement.querySelector('.taxRateSettings__addButton').addEventListener('click', function (event) {
        event.preventDefault();
        table.rates.push(JSON.parse(newRowJson));
    });

    function updateAddressVisibility() {
        tableStyle.setProperty('--tax-address-display', checkbox.checked ? 'none' : 'table-cell');
    }

    checkbox.addEventListener('change', updateAddressVisibility);
    updateAddressVisibility();

    const form = document.querySelector(`form:has(${vueQuery})`);
});
