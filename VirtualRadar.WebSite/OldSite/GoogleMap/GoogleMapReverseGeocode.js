function GoogleMapReverseGeocode(events)
{
    var that = this;

    var mEvents = events;
    var mGeocoder = new google.maps.Geocoder();

    var mConstPositionLookupDisabled = "Position lookup disabled";
    var mGeocodeEnabled = false;
    var mLastGeocodeAircraft = -1;
    var mLastGeocodeLat;
    var mLastGeocodeLong;
    var mGeocodeInProgress = false;
    var mLastGeocodeResponse = mConstPositionLookupDisabled;

    this.getLastResponse = function() { return mLastGeocodeResponse; }

    mEvents.addListener(EventId.aircraftDetailRefresh, refreshDetailHandler);
    mEvents.addListener(EventId.toggleReverseGeocode, toggleReverseGeocodeHandler);

    function refreshDetailHandler(sender, aircraft)     { that.refresh(aircraft.getHasPos(), aircraft.Id, aircraft.Lat, aircraft.Long); };
    function toggleReverseGeocodeHandler(sender, args)  { that.toggleOnOff(); };

    this.refresh = function(hasPosition, aircraftId, latitude, longitude)
    {
        if(mLastGeocodeAircraft != -1 && mLastGeocodeAircraft != aircraftId) mLastGeocodeResponse = mGeocodeEnabled ? "" : mConstPositionLookupDisabled;

        if(hasPosition === true && mGeocodeEnabled !== false && !mGeocodeInProgress && (aircraftId !== mLastGeocodeAircraft || latitude !== mLastGeocodeLat || longitude !== mLastGeocodeLong)) {
            mGeocodeInProgress = true;
            mLastGeocodeAircraft = aircraftId;
            mLastGeocodeLat = latitude;
            mLastGeocodeLong = longitude;

            mGeocoder.geocode({'latLng': new google.maps.LatLng(latitude, longitude)}, function(results, status) {
                switch(status) {
                    case google.maps.GeocoderStatus.OK:
                        if(results.length > 0) {
                            if(mLastGeocodeAircraft === aircraftId && mGeocodeEnabled) mLastGeocodeResponse = results[0].formatted_address;
                        }
                        break;
                    case google.maps.GeocoderStatus.ZERO_RESULTS:
                        if(mLastGeocodeAircraft === aircraftId && mGeocodeEnabled) mLastGeocodeResponse = "No address found";
                        break;
                    case google.maps.GeocoderStatus.OVER_QUERY_LIMIT:
                        mLastGeocodeResponse = "Ooops! Exceeded the lookup quota!";
                        mGeocodeEnabled = false;
                        break;
                    default:
                        mLastGeocodeResponse = status;
                        break;
                }
                mGeocodeInProgress = false;
                mEvents.raise(EventId.reverseGeocodeResponseChanged, this, mLastGeocodeResponse);
            });
        }
    };

    this.toggleOnOff = function()
    {
        if(mGeocodeEnabled) {
            mGeocodeEnabled = false;
            mLastGeocodeResponse = mConstPositionLookupDisabled;
        } else {
            mGeocodeEnabled = true;
            mLastGeocodeResponse = "";
        }
        mEvents.raise(EventId.reverseGeocodeResponseChanged, this, mLastGeocodeResponse);
    };
}