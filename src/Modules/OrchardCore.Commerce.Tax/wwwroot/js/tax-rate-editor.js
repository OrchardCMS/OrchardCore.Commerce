document.querySelectorAll('.taxRateSettings').forEach((element) => {
    function hideAddressVisibility(value) {
        element.style.setProperty('--tax-address-display', value ? 'none' : 'table-cell');
    }

    const newRowJson = element.getAttribute('data-new-row');
    new window.Vue({
        el: element,
        data: {
            hideAddressColumns: true,
            rates: JSON.parse(element.getAttribute('data-rates')),
        },
        computed: {
            json: (self) => JSON.stringify(self.rates),
        },
        methods: {
            addRow() {
                this.rates.push(JSON.parse(newRowJson));
            },
        },
        watch: {
            hideAddressColumns(value) {
                hideAddressVisibility(value);
            },
        },
    });

    hideAddressVisibility(true);
});
