var _TheFatController;

function initialise()
{
    _TheFatController = new FatController();
    _TheFatController.start();
}

function FatController()
{
    var that = this;

    var mEvents;
    var mOptions;
    var mOptionsStorage;
    var mOptionsTabPages;
    var mOptionsUI;
    var mCurrentLocation;
    var mCurrentLocationStorage;
    var mCurrentLocationUI;
    var mMap;
    var mMapNorthEast;
    var mMapSouthWest;
    var mMovingMapControl;
    var mMapMovingAutomatically = false;
    var mMapInfoButton;
    var mMarkers;
    var mSidebar;
    var mAircraftCollection;
    var mAircraftList;
    var mAircraftListOptions;
    var mAircraftDetail;
    var mReverseGeocode;
    var mFullListXhr;
    var mHttpRequestTimeout = 10000;
    var mSiteTimeout;
    var mAudio;
    var mIPhoneMapPages;
    var mIPhoneMapPlaneList;
    var mIPhoneMapAircraftDetail;
    var mIPhoneMapOptionsUI;
    var mIPhoneInfoWindow;
    var mGoogleMapGeolocation;
    var mGoogleMapGotoCurrentLocationButton;
    var mPaused = false;

    this.start = function()
    {
        mEvents = new Events();

        mFullListXhr = new XHR();
        mFullListXhr.setTimeoutCallback(showAircraftTimedOutHandler);

        var pageParams = getPageParameters();
        var debugShowPointCount = getPageParameterValue(pageParams, 'showpointcount') !== null;
        var debugFixedColumnWidths = getPageParameterValue(pageParams, 'fixedColumnWidths') !== null;

        mOptionsStorage = new GoogleMapOptionsStorage();
        mOptions = mOptionsStorage.load();
        mOptionsStorage.save(mOptions);
        if(typeof(GoogleMapOptionsUI) !== 'undefined') mOptionsUI = new GoogleMapOptionsUI(mEvents, mOptions);

        if(isIphone() || isIpad()) {
            window.addEventListener('orientationchange', orientationChangedHandler);
        }

        if(typeof(GoogleMapTimeout) !== 'undefined') mSiteTimeout = new GoogleMapTimeout(mOptions, mEvents);

        if(typeof(GoogleMapSidebar) !== 'undefined') {
            mSidebar = new GoogleMapSidebar(mEvents);
            mSidebar.modifyUI();
        }

        var mapLatitude = mOptions.mapLatitude;
        var mapLongitude = mOptions.mapLongitude;
        var mapType = mOptions.mapType;
        var mapZoom = mOptions.mapZoom;

        var mapTypeControlStyle = _MapMode === MapMode.iPhone ? google.maps.MapTypeControlStyle.DROPDOWN_MENU : google.maps.MapTypeControlStyle.DEFAULT;
        var mapTypeControlPosition = _MapMode === MapMode.iPhone ? google.maps.ControlPosition.TOP_LEFT : google.maps.ControlPosition.TOP_RIGHT;
        mMap = new google.maps.Map(document.getElementById('map_canvas'), {
            zoom: mapZoom,
            center: new google.maps.LatLng(mapLatitude, mapLongitude),
            mapTypeId: mapType,
            scaleControl: true,
            streetViewControl: false,
            mapTypeControlOptions: { style: mapTypeControlStyle, position: mapTypeControlPosition }
        });
        google.maps.event.addListener(mMap, "bounds_changed", mapBoundsChangedHandler);
        google.maps.event.addListener(mMap, "dragend", mapSettingsChangedHandler);
        google.maps.event.addListener(mMap, "maptypeid_changed", mapSettingsChangedHandler);
        google.maps.event.addListener(mMap, "zoom_changed", mapZoomChangedHandler);

        if(_MapMode !== MapMode.flightSim && typeof(GoogleMapCurrentLocation) !== 'undefined') {
            mEvents.addListener(EventId.currentLocationChanged, currentLocationChangedHandler);
            mCurrentLocationStorage = new GoogleMapCurrentLocationStorage(mEvents);
            mCurrentLocation = mCurrentLocationStorage.load();
            mCurrentLocationStorage.save(mCurrentLocation);
            if(_MapMode !== MapMode.iPhone) mCurrentLocationUI = new GoogleMapCurrentLocationUI(mEvents, mCurrentLocation, mMap);
        }

        mAircraftCollection = new GoogleMapAircraftCollection(mEvents, mOptions);
        mMarkers = new GoogleMapMarkerCollection(mEvents, mMap, mAircraftCollection, mOptions);
        if(typeof(GoogleMapAircraftList) !== 'undefined') {
            mAircraftList = new GoogleMapAircraftList(mEvents, mAircraftCollection, mOptions, mSidebar);
        }
        if(typeof(GoogleMapAircraftListOptions) !== 'undefined') {
            mAircraftListOptions = new GoogleMapAircraftListOptions(mEvents, mAircraftList, mAircraftList !== undefined ? mAircraftList.getListColumns() : undefined);
            mAircraftListOptions.loadSortSettings();
        }
        if(typeof(GoogleMapReverseGeocode) !== 'undefined') mReverseGeocode = new GoogleMapReverseGeocode(mEvents);
        if(typeof(GoogleMapAircraftDetail) !== 'undefined') mAircraftDetail = new GoogleMapAircraftDetail(_MapMode, mEvents, mOptions, mAircraftCollection, mReverseGeocode !== undefined, mCurrentLocation !== undefined, mCurrentLocation !== null);
        if(typeof(GoogleMapAudio) !== 'undefined') mAudio = new GoogleMapAudio(mMap, mEvents, mOptions);

        if(typeof(GoogleMapMovingMapControl) !== 'undefined') {
            mMovingMapControl = new GoogleMapMovingMapControl(mEvents, mOptions);
            mMovingMapControl.addToMap(mMap);
        }

        if(typeof(GoogleMapGotoCurrentLocationButton) !== 'undefined') {
            mGoogleMapGotoCurrentLocationButton = new GoogleMapGotoCurrentLocationButton(mEvents);
            mGoogleMapGotoCurrentLocationButton.addToMap(mMap);
            if(mCurrentLocation) mGoogleMapGotoCurrentLocationButton.setVisible(true);
            mEvents.addListener(EventId.gotoCurrentLocationClicked, gotoCurrentLocationClicked);
        }

        if(typeof(GoogleMapGeolocation) !== 'undefined') {
            mGoogleMapGeolocation = new GoogleMapGeolocation(mEvents, mMap, mCurrentLocation);
            mGoogleMapGeolocation.initialise();
            mEvents.addListener(EventId.geolocationUpdated, geolocationUpdatedHandler);
        }

        mOptionsTabPages = new GoogleMapOptionsTabPages(mEvents, mOptions, mOptionsStorage, mCurrentLocationUI, mAudio, mMovingMapControl);

        if(mOptionsUI !== undefined) {
            mOptionsUI.addToPage();
            mOptionsUI.copyOptionsToForm();
        }

        mEvents.addListener(EventId.aircraftMarkerClicked, aircraftMarkerClickedHandler);
        mEvents.addListener(EventId.optionsResetMapClicked, resetMapHandler);
        mEvents.addListener(EventId.sidebarResized, sidebarResizedHandler);
        mEvents.addListener(EventId.refreshMovingMapPosition, movingMapUpdateHandler);
        mEvents.addListener(EventId.selectClosestClicked, selectClosestClickedHandler);
        mEvents.addListener(EventId.togglePauseClicked, togglePauseClickedHandler);

        if(debugShowPointCount) {
            if(mAircraftList !== undefined) mAircraftList.setDebugCountPoints(true);
            if(mAircraftDetail !== undefined) mAircraftDetail.setDebugCountPoints(true);
        }
        if(debugFixedColumnWidths) {
            if(mAircraftList !== undefined) mAircraftList.setDebugFixedColumnWidths(true);
        }

        addIPhoneMapElements();

        writeCredits();

        setTimeout(showAircraft, 1000);  // <-- give the map time to initialise before we use it, getBounds() won't work if we call it straight away
    };

    function addIPhoneMapElements()
    {
        if(typeof(GoogleMapInfoButton) !== 'undefined') {
            mMapInfoButton = new GoogleMapInfoButton(mEvents, mOptions);
            mMapInfoButton.addToMap(mMap);
            mEvents.addListener(EventId.infoButtonClicked, googleMapInfoButtonClicked);
        }

        if(typeof(iPhoneMapPages) !== 'undefined') {
            mIPhoneMapPages = new iPhoneMapPages(mEvents);
            mIPhoneMapPages.initialise();
            mEvents.addListener(EventId.iPhonePageSwitched, iPhonePageSwitchedHandler);

            mEvents.addListener(EventId.aircraftMarkerClicked, iPhoneMapAircraftMarkerClicked);
        }

        if(typeof(iPhoneMapAircraftDetail) !== 'undefined') {
            mIPhoneMapAircraftDetail = new iPhoneMapAircraftDetail(mEvents, mOptions, mIPhoneMapPages);
            mIPhoneMapAircraftDetail.initialise();
            if(mCurrentLocation) mIPhoneMapAircraftDetail.setLocationKnown(true);
        }

        if(typeof(iPhoneMapPlaneList) !== 'undefined') {
            mIPhoneMapPlaneList = new iPhoneMapPlaneList(mEvents, mOptions, mIPhoneMapPages, mIPhoneMapAircraftDetail, mAircraftCollection);
            mIPhoneMapPlaneList.initialise();
        }

        if(typeof(iPhoneMapOptionsUI) !== 'undefined') {
            mIPhoneMapOptionsUI = new iPhoneMapOptionsUI(mEvents, mOptions, mOptionsStorage, mAircraftListOptions, mIPhoneMapPages);
            mIPhoneMapOptionsUI.initialise();
        }

        if(typeof(iPhoneMapInfoWindow) !== 'undefined') {
            mIPhoneInfoWindow = new iPhoneMapInfoWindow(mEvents, mMap, mIPhoneMapPages, mIPhoneMapAircraftDetail);
            mIPhoneInfoWindow.initialise();
        }
    };

    function showAircraft()
    {
        if(mSiteTimeout === undefined || !mSiteTimeout.isTimedOut()) {
            if(mPaused) setTimeout(showAircraft, (1 + mOptions.refreshSeconds) * 1000);
            else {
                var headers = [];
                headers.push({name: 'X-VirtualRadarServer-AircraftIds', value: mAircraftCollection.getAircraftIdListParameter() });
                mFullListXhr.beginSend('GET', getAircraftListAddress(false), null, null, mHttpRequestTimeout, showAircraftHandler, null, headers);
            }
        }
    };

    function getAircraftListAddress()
    {
        var result = _MapMode === MapMode.flightSim ? "FlightSimList.json" : "AircraftList.json";
        result += "?tn=1"; // Did have time parameter on here to prevent caching but server now sends proper Cache-Control
        if(_MapMode !== MapMode.flightSim) result += "&" + currentLocationParameters();
        if(mAircraftListOptions !== undefined) {
            result += "&sortBy1=" + mAircraftListOptions.getSortField1() +
                      "&sortOrder1=" + mAircraftListOptions.getSortDirection1() +
                      "&sortBy2=" + mAircraftListOptions.getSortField2() +
                      "&sortOrder2=" + mAircraftListOptions.getSortDirection2();
        }
        if(mOptions.tempRefreshTrails) {
            result += '&refreshTrails=1';
            mOptions.tempRefreshTrails = false;
        }
        result += '&trFmt=' + (mOptions.showShortTrail ? 's' : 'f');
        result += filterParameters();
        result += '&' + lastDataVersionParameter();

        return result;
    };

    function lastDataVersionParameter() { return "ldv=" + mAircraftCollection.getDataVersion(); };
    function currentLocationParameters()
    {
        var result = '';

        if(mCurrentLocation !== undefined) {
            var latitude;
            var longitude;
            if(mCurrentLocation !== null) {
                latitude = mCurrentLocation.getLat();
                longitude = mCurrentLocation.getLng();
            } else {
                var centreMap = mMap.getCenter();
                latitude = centreMap.lat();
                longitude = centreMap.lng();
            }

            result = 'lat=' + latitude + '&lng=' + longitude;
        }

        return result;
    };
    function filterParameters()
    {
        var result = '';

        if(mOptions.filterAircraftNotOnMap) {
            if(mMapNorthEast && mMapSouthWest) {
                result += '&fNBnd=' + mMapNorthEast.lat() + '&fEBnd=' + mMapNorthEast.lng();
                result += '&fSBnd=' + mMapSouthWest.lat() + '&fWBnd=' + mMapSouthWest.lng();
            }
            result += '&fInBnds=1';
        }

        if(mOptions.filterAircraftWithNoPosition) result += '&fNoPos=1';
        if(mOptions.filteringEnabled) {
            result += '&fHgtUnit=' + mOptions.heightUnit;
            result += '&fDstUnit=' + mOptions.distanceUnit;
            if(mOptions.filterAltitudeLower) result += '&fAltL=' + encodeURIComponent(mOptions.filterAltitudeLower);
            if(mOptions.filterAltitudeUpper) result += '&fAltU=' + encodeURIComponent(mOptions.filterAltitudeUpper);
            if(mOptions.filterDistanceLower) result += '&fDstL=' + encodeURIComponent(mOptions.filterDistanceLower);
            if(mOptions.filterDistanceUpper) result += '&fDstU=' + encodeURIComponent(mOptions.filterDistanceUpper);
            if(mOptions.filterSquawkLower) result += '&fSqkL=' + encodeURIComponent(mOptions.filterSquawkLower);
            if(mOptions.filterSquawkUpper) result += '&fSqkU=' + encodeURIComponent(mOptions.filterSquawkUpper);
            if(mOptions.filterCallsign) result += '&fCallC=' + encodeURIComponent(mOptions.filterCallsign);
            if(mOptions.filterOperator) result += '&fOpC=' + encodeURIComponent(mOptions.filterOperator);
            if(mOptions.filterRegistration) result += '&fRegC=' + encodeURIComponent(mOptions.filterRegistration);
            if(mOptions.filterType) result += '&fTypS=' + encodeURIComponent(mOptions.filterType);
            if(mOptions.filterWtc) result += '&fWtcQ=' + encodeURIComponent(mOptions.filterWtc);
            if(mOptions.filterSpecies) result += '&fSpcQ=' + encodeURIComponent(mOptions.filterSpecies);
            if(mOptions.filterEngType) result += '&fEgtQ=' + encodeURIComponent(mOptions.filterEngType);
            if(mOptions.filterIsMilitary) result += '&fMilQ=1';
            if(mOptions.filterIsInteresting) result += '&fIntQ=1';
            if(mOptions.filterCountry) result += '&fCouC=' + encodeURIComponent(mOptions.filterCountry);
        }

        return result;
    }

    function showAircraftHandler(status, responseText)
    {
        if(status !== 200) {
            // Wait a little while before trying again - don't want to hammer it if it's not there
            setTimeout(showAircraft, 15 * 1000);
        } else {
            var content = eval('(' + responseText + ')');

            applyAircraftToMap(content);

            // Wait and try again. Note that the refresh period is 1 second higher than requested - this
            // is for historical reasons to keep default behaviour after a bug that doubled the refresh
            // time was fixed. To compensate the minimum refresh rate is now allowed to be zero (but it
            // still defaults to 1).
            setTimeout(showAircraft, (1 + mOptions.refreshSeconds) * 1000);
        }
    };

    function showAircraftTimedOutHandler(xhr)
    {
        // It's unlikely that this will ever be called, but just in case...
        setTimeout(showAircraft, 15 * 1000);
    };

    function applyAircraftToMap(content)
    {
        mOptions.tempServerTime = new Date(content.stm);
        if(content.flgH) mOptions.tempFlagHeight = content.flgH;
        if(content.flgW) mOptions.tempFlagWidth = content.flgW;
        if(content.shtTrlSec) mOptions.tempServerShortTrailLengthSeconds = content.shtTrlSec;
        if(content.showSil !== undefined) mOptions.tempShowSilhouettes = content.showSil;
        if(content.showFlg !== undefined) mOptions.tempShowFlags = content.showFlg;
        if(content.showPic !== undefined) mOptions.tempShowPictures = content.showPic;
        mOptions.tempAvailableAircraft = content.totalAc ? content.totalAc : content.acList.length;

        mAircraftCollection.parseAircraftList(content);

        switch(_MapMode) {
            case MapMode.normal:        document.title = 'Virtual Radar (' + content.acList.length + (mOptions.tempAvailableAircraft == content.acList.length ? '' : '/' + mOptions.tempAvailableAircraft) + ') - Live Map'; break;
            case MapMode.flightSim:     document.title = 'Virtual Radar - Flight Simulator Map'; break;
            default:                    break;
        }
    };

    function writeCredits()
    {
        var creditElement = document.getElementById('credit');
        if(creditElement !== null) creditElement.innerHTML = '<a href="http://www.virtualradarserver.co.uk/" target="vrs" title="Version ' + _ServerConfig.vrsVersion + '">Powered by Virtual Radar Server</a>';
    };

    function aircraftMarkerClickedHandler(sender, aircraft)
    {
        mAircraftCollection.selectAircraft(aircraft, true);
    };

    function resetMapHandler()
    {
        mMap.setOptions({
            zoom: _ServerConfig.initialZoom,
            center: new google.maps.LatLng(_ServerConfig.initialLatitude, _ServerConfig.initialLongitude),
            mapTypeId: _ServerConfig.initialMapType
        });
        mOptions.mapLongitude = mOptions.mapLatitude = mOptions.mapType = mOptions.mapZoom = null;
        mOptionsStorage.save(mOptions);
    };

    function mapZoomChangedHandler()
    {
        mapSettingsChangedHandler();
        mEvents.raise(EventId.zoomChanged, null, null);
    };

    function mapSettingsChangedHandler()
    {
        if(!mMapMovingAutomatically) {
            var centre = mMap.getCenter();
            mOptions.mapLongitude = centre.lng();
            mOptions.mapLatitude = centre.lat();
            mOptions.mapType = mMap.getMapTypeId();
            mOptions.mapZoom = mMap.getZoom();
            mOptionsStorage.save(mOptions);
            mEvents.raise(EventId.resetTimeOut, null, null);
        }
    };

    function mapBoundsChangedHandler()
    {
        var bounds = mMap.getBounds();
        if(!bounds) {
            mMapNorthEast = undefined;
            mMapSouthWest = undefined;
        } else {
            // Need to guard against a zero-sized bounds - this happened on the iPad / iPhone when the map div was hidden
            var northEast = bounds.getNorthEast();
            var southWest = bounds.getSouthWest();
            if(northEast.lat() != southWest.lat() || northEast.lng() != southWest.lng()) {
                mMapNorthEast = northEast;
                mMapSouthWest = southWest;
            }
        }
    };

    function currentLocationChangedHandler(sender, args)
    {
        mCurrentLocation = args;
        mCurrentLocationStorage.save(mCurrentLocation);
        if(_MapMode !== MapMode.iPhone) mEvents.raise(EventId.resetTimeOut, null, null);

        if(mIPhoneMapAircraftDetail !== undefined) mIPhoneMapAircraftDetail.setLocationKnown(true);
        if(mGoogleMapGeolocation !== undefined) mGoogleMapGeolocation.setShowMarker(true);
        if(mGoogleMapGotoCurrentLocationButton !== undefined && mGoogleMapGotoCurrentLocationButton.getVisible() === false) mGoogleMapGotoCurrentLocationButton.setVisible(true);
    };

    function selectClosestClickedHandler(sender, args)
    {
        var aircraft = mAircraftCollection.getAutoSelectAircraft();
        if(aircraft !== null) mAircraftCollection.selectAircraft(aircraft, false);
    };

    function sidebarResizedHandler(sender, args)
    {
        if(mMap !== null) google.maps.event.trigger(mMap, 'resize');
    };

    function movingMapUpdateHandler(sender, args)
    {
        var movingMap = sender;
        var aircraft = mAircraftCollection.getSelectedAircraft();
        if(aircraft !== null && aircraft.OnRadar) {
            mMapMovingAutomatically = true;
            aircraft.centreOnMap(mMap);
            mMapMovingAutomatically = false;
        }
    };

    function orientationChangedHandler()
    {
        if(mMap !== null) google.maps.event.trigger(mMap, 'resize');
        mEvents.raise(EventId.orientationChanged, null, window.orientation);
    };

    function googleMapInfoButtonClicked(sender, args)
    {
        if(mIPhoneMapPages !== undefined) mIPhoneMapPages.select(PageId.aircraftList, PageAnimation.none);
    };

    function iPhonePageSwitchedHandler(sender, args)
    {
        if(mIPhoneMapPages.getSelectedPageId() === PageId.map) {
            if(mMap !== null) {
                google.maps.event.trigger(mMap, 'resize');
            }
        }
    };

    function iPhoneMapAircraftMarkerClicked(sender, aircraft)
    {
        if(mIPhoneInfoWindow !== undefined) mIPhoneInfoWindow.displayAircraftInformation(aircraft);
    };

    function geolocationUpdatedHandler(sender, args)
    {
        var geoloc = sender;
        if(mCurrentLocation === undefined || mCurrentLocation === null) {
            mCurrentLocation = new GoogleMapCurrentLocation(mEvents, geoloc.getLatitude(), geoloc.getLongitude(), true);
        } else {
            mCurrentLocation.setPosition(geoloc.getLatitude(), geoloc.getLongitude());
        }
    };

    function gotoCurrentLocationClicked(sender, args)
    {
        if(mCurrentLocation !== undefined && mCurrentLocation !== null) {
            mMap.setCenter(mCurrentLocation.getGoogleLatLng());
            mapSettingsChangedHandler();  // <-- this wasn't being raised by Google Maps...
        }
    };

    function togglePauseClickedHandler(sender, args)
    {
        mPaused = !mPaused;
        mEvents.raise(EventId.pauseChanged, null, mPaused);
    };
}