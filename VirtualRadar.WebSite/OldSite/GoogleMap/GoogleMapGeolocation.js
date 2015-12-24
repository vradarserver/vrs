function GoogleMapGeolocation(events, map, currentLocation)
{
    var that = this;
    var mEvents = events;
    var mMap = map;
    var mMarker;
    this.getShowMarker = function() { return mMarker === undefined ? false : mMarker.getVisible(); };
    this.setShowMarker = function(value) { if(mMarker !== undefined) mMarker.setVisible(value); };
    var mEnabled = false;
    var mWatchHandle;
    this.getEnabled = function() { return mEnabled; };
    var mLatitude = 0;
    var mLongitude = 0;
    var mCurrentLocation = currentLocation;
    this.getLatitude = function() { return mLatitude; };
    this.getLongitude = function() { return mLongitude; };
    this.getHasPos = function() { return mLatitude !== undefined && mLongitude !== undefined; };
    this.getGoogleLatLng = function() { return new google.maps.LatLng(mLatitude, mLongitude); };

    this.initialise = function()
    {
        if(navigator.geolocation) {
            mEnabled = true;

            if(mCurrentLocation !== null && mCurrentLocation !== undefined) {
                mLatitude = mCurrentLocation.getLat();
                mLongitude = mCurrentLocation.getLng();
            }

            if(mMap !== undefined && mMap !== null) {
                mMarker = new google.maps.Marker({
                        position: new google.maps.LatLng(mLatitude, mLongitude),
                        map: mMap,
                        clickable: false,
                        flat: true,
                        icon: new google.maps.MarkerImage('Images/YouAreHere.png', new google.maps.Size(10, 10), new google.maps.Point(0, 0), new google.maps.Point(5, 5)),
                        visible: mCurrentLocation !== null && mCurrentLocation !== undefined,
                        zIndex: 1
                });
            }

            mWatchHandle = navigator.geolocation.watchPosition(updateHandler, errorHandler);
        }
    };

    function updateHandler(position)
    {
        if(position.coords.latitude !== mLatitude || position.coords.longitude !== mLongitude) {
            mLatitude = position.coords.latitude;
            mLongitude = position.coords.longitude;

            if(mMarker !== undefined) mMarker.setPosition(that.getGoogleLatLng());

            mEvents.raise(EventId.geolocationUpdated, that, null);
        }
    };

    function errorHandler(error)
    {
    };
}