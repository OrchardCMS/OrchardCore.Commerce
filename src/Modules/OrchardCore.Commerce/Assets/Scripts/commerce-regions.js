let commerceRegions;

function commerceRegionsInitialize(regionData) {
    commerceRegions = regionData;
}

function commerceRegionsBind(provinceDropDown, regionDropDown) {
    $(regionDropDown).change(() => {
        commerceRegionsOnChange(provinceDropDown, regionDropDown);
    });
}

function commerceRegionsOnChange(provinceDropDown, regionDropDown) {
    const $province = $(provinceDropDown);
    $province.empty();
    const regionName = $(regionDropDown).val();
    const region = commerceRegions[regionName] || { '--': "---" };
    $.each(Object.getOwnPropertyNames(region), function () {
        $province.append($('<option/>').val(this).text(region[this]));
    });
}
