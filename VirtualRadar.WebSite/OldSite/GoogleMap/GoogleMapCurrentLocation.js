var _SingletonGoogleMapCurrentLocationUI;
function toggleDisplayCurrentLocation() { _SingletonGoogleMapCurrentLocationUI.toggleDisplay(); }

function GoogleMapCurrentLocation(events, lat, lng, enabled)
{
    var that = this;
    var mEnabled = enabled;
    var mLat = lat;
    var mLng = lng;
    var mEvents = events;

    this.getEnabled = function()      { return mEnabled; };
    this.getLat = function()          { return mLat; };
    this.getLng = function()          { return mLng; };
    this.getGoogleLatLng = function() { return new google.maps.LatLng(mLat, mLng); };

    this.setEnabled = function(value)
    {
        if(value !== mEnabled) {
            mEnabled = value;
            mEvents.raise(EventId.currentLocationChanged, this, this);
        }
    };

    this.setPosition = function(lat, lng)
    {
        if(mLat !== lat || mLng !== lng) {
            mLat = lat;
            mLng = lng;
            mEvents.raise(EventId.currentLocationChanged, this, this);
        }
    };
}

function GoogleMapCurrentLocationStorage(events)
{
    var that = this;
    var mEvents = events;

    function eraseOldCookies()
    {
        eraseCookie("gmLocLatitude");
        eraseCookie("gmLocLongitude");
        eraseCookie("gmLocEnabled");
    };

    this.save = function(location)
    {
        eraseOldCookies();

        var nameValues = new nameValueCollection();
        nameValues.pushValue('lat', location === null ? null : location.getLat());
        nameValues.pushValue('lng', location === null ? null : location.getLng());
        nameValues.pushValue('enb', location === null ? null : location.getEnabled());
        writeCookie('googleMapHomePin', nameValues.toString());
    };

    this.load = function()
    {
        var result = null;

        var cookieValues = readCookieValues();

        // Try the obsolete cookies first
        var latitude = extractCookieValue(cookieValues, "gmLocLatitude");
        var longitude = extractCookieValue(cookieValues, "gmLocLongitude");
        var enabled = extractCookieValue(cookieValues, "gmLocEnabled");

        // ... and then override them with the real cookie if it exists
        var nameValues = new nameValueCollection()
        nameValues.fromString(extractCookieValue(cookieValues, 'googleMapHomePin'));
        if(nameValues.getLength() > 0) {
            latitude = nameValues.getValue('lat');
            longitude = nameValues.getValue('lng');
            enabled = nameValues.getValue('enb');
        }

        if(latitude !== null && longitude !== null && enabled !== null) {
            result = new GoogleMapCurrentLocation(mEvents, Number(latitude), Number(longitude), enabled !== "false");
        }

        return result;
    };
}

function GoogleMapCurrentLocationUI(events, location, map)
{
    var that = this;
    _SingletonGoogleMapCurrentLocationUI = this;
    var mEvents = events;
    var mOptionsUI;
    var mLocation = location;
    var mMap = map;
    var mMarker = new google.maps.Marker({
            position: new google.maps.LatLng(0, 0),
            draggable: true
    });
    google.maps.event.addListener(mMarker, "dragend", currentLocationDraggedHandler);

    if(mLocation !== null) {
        mMarker.setPosition(mLocation.getGoogleLatLng());
        if(mLocation.getEnabled()) mMarker.setMap(mMap);
    }

    this.tabPageHtml = function(optionsUI, tabName)
    {
        mOptionsUI = optionsUI;
        var html = optionsUI.htmlHeading('Current location');
        html += optionsUI.htmlCheckBox(tabName, 'clocEnableCurrentLocation', 'Show current location on map', 'toggleDisplayCurrentLocation()') + optionsUI.htmlEol();

        return html;
    };

    this.copyOptionsToForm = function(form)
    {
        form.clocEnableCurrentLocation.checked = mLocation !== null && mLocation.getEnabled();
    };

    this.copyFormToOptions = function(form)
    {
    };

    this.toggleDisplay = function()
    {
        toggleCurrentLocation(mOptionsUI.getForm().clocEnableCurrentLocation.checked);
    }

    function toggleCurrentLocation(turnOn)
    {
        if(!turnOn) {
            mMarker.setMap(null);
            mLocation.setEnabled(false);
        } else {
            createLocation();
            mMarker.setPosition(mLocation.getGoogleLatLng());
            mMarker.setMap(mMap);
            mLocation.setEnabled(true);
        }
    };

    function createLocation()
    {
        if(mLocation === null) {
            var centreMap = mMap.getCenter();
            mLocation = new GoogleMapCurrentLocation(mEvents, centreMap.lat(), centreMap.lng(), true);
            mEvents.raise(EventId.currentLocationChanged, this, mLocation);
        }
    };

    function currentLocationDraggedHandler(args)
    {
        var latLng = args.latLng;
        mLocation.setPosition(latLng.lat(), latLng.lng());
        mMarker.setPosition(latLng);
    };
}