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
    var provinceEl = $(provinceDropDown);
    provinceEl.empty();
    var regionName = $(regionDropDown).val();
    var region = commerceRegions[regionName];
    if (region) {
        $.each(Object.getOwnPropertyNames(region), function () {
            provinceEl.append($("<option/>").val(this).text(region[this]));
        });
    }
}