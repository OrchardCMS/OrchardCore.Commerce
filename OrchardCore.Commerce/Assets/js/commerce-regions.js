var commerceRegions;

function commerceRegionsInitialize(regionData) {
    commerceRegions = regionData;
}

function commerceRegionsBind(provinceDropDown, regionDropDown) {
    $(regionDropDown).change(function () {
        commerceRegionsOnChange(provinceDropDown, regionDropDown);
    });
}

function commerceRegionsOnChange(provinceDropDown, regionDropDown) {
    var $province = $(provinceDropDown);
    $province.empty();
    var regionName = $(regionDropDown).val();
    var region = commerceRegions[regionName];
    if (region) {
        $.each(Object.getOwnPropertyNames(region), function () {
            $province.append($("<option/>").val(this).text(region[this]));
        });
    }
}
