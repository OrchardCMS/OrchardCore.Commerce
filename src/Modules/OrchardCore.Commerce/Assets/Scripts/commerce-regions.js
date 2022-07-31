let commerceRegions;

// "Function is defined but never used."
// It's used in a view.
// eslint-disable-next-line
function commerceRegionsInitialize(regionData) {
    commerceRegions = regionData;
}

function commerceRegionsOnChange(provinceDropDown, regionDropDown) {
    const $province = $(provinceDropDown);
    $province.empty();
    const regionName = $(regionDropDown).val();
    const region = commerceRegions[regionName] || { '--': '---' };
    $.each(Object.getOwnPropertyNames(region), () => {
        $province.append($('<option/>').val(this).text(region[this]));
    });
}

// Same as above.
// eslint-disable-next-line
function commerceRegionsBind(provinceDropDown, regionDropDown) {
    $(regionDropDown).change(() => {
        commerceRegionsOnChange(provinceDropDown, regionDropDown);
    });
}
