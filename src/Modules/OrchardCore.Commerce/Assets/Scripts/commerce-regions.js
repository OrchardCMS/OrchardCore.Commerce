let commerceRegions = { };

// "Function is defined but never used."
// It's used in a view.
// eslint-disable-next-line
function commerceRegionsInitialize(regionData) {
    commerceRegions = regionData;
}

function commerceRegionsOnChange(provinceDropDown, container, regionDropDown) {
    const regionName = $(regionDropDown).val();
    const provinces = commerceRegions[regionName] || { '': '---' };
    const provinceIds = Object.keys(provinces);

    const $province = $(provinceDropDown).empty();
    const $container = $province.closest(container);

    // Update dropdown.
    provinceIds.forEach((id) => $('<option/>').val(id).text(provinces[id]).appendTo($province));
    $province.val(provinceIds[0]);

    // If there are no provinces, hide the whole row.
    const hasProvinces = provinceIds[0] !== '';
    $container.toggle(hasProvinces);
    $province.prop('required', hasProvinces);
}

// Same as above.
// eslint-disable-next-line
function commerceRegionsBind(provinceDropDown, container, regionDropDown, provinceValue) {
    $(regionDropDown)
        .change(() => commerceRegionsOnChange(provinceDropDown, container, regionDropDown))
        .change();
    $(provinceDropDown)
        .val(provinceValue)
        .change();
}
