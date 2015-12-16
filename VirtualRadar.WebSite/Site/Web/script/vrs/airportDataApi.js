var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.airportDataApiThumbnailsUrl = VRS.globalOptions.airportDataApiThumbnailsUrl || 'AirportDataThumbnails.json';
    VRS.globalOptions.airportDataApiTimeout = VRS.globalOptions.airportDataApiTimeout || 10000;
    var AirportDataApi = (function () {
        function AirportDataApi() {
        }
        AirportDataApi.prototype.getThumbnails = function (icao, registration, countThumbnails, callback) {
            $.ajax({
                url: VRS.globalOptions.airportDataApiThumbnailsUrl,
                dataType: 'json',
                data: { icao: icao, reg: registration, numThumbs: countThumbnails },
                error: function (jqXHR, textStatus) {
                    callback(icao, { status: jqXHR.status, error: 'XHR call failed: ' + textStatus });
                },
                success: function (data) {
                    callback(icao, data);
                },
                timeout: VRS.globalOptions.airportDataApiTimeout
            });
        };
        return AirportDataApi;
    })();
    VRS.AirportDataApi = AirportDataApi;
})(VRS || (VRS = {}));
//# sourceMappingURL=airportDataApi.js.map