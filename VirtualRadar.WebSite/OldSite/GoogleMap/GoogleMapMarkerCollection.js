var GoogleMapMarkerCollectionObjects = [];

function googleMapMarkerCollection_MarkerClickHandler(event)
{
    if(GoogleMapMarkerCollectionObjects.length != 1) throw('Only a single map marker collection is supported');
    GoogleMapMarkerCollectionObjects[0].markerClickHandler(this);
}

function GoogleMapMarkerCollection(events, map, aircraftCollection, options)
{
    var that = this;
    var mGlobalIndex = GoogleMapMarkerCollectionObjects.length;
    GoogleMapMarkerCollectionObjects.push(that);

    var mOldMarkers = [];
    var mEvents = events;
    var mOptions = options;
    var mAircraftCollection = aircraftCollection;
    var mMap = map;

    mEvents.addListener(EventId.acListRefreshed, aircraftListRefreshedHandler);
    mEvents.addListener(EventId.noLongerTracked, aircraftNoLongerTrackedHandler);
    mEvents.addListener(EventId.acDeselected, aircraftSelectionChangedHandler);
    mEvents.addListener(EventId.acSelected, aircraftSelectionChangedHandler);
    mEvents.addListener(EventId.optionsChanged, optionsChangedHandler);
    mEvents.addListener(EventId.debugGetCountPoints, getDebugCountPoints);
    mEvents.addListener(EventId.debugGetPointText, getDebugPointText);
    mEvents.addListener(EventId.zoomChanged, zoomChangedHandler);

    function aircraftListRefreshedHandler(sender, args) { refreshMarkers(args); }
    function aircraftNoLongerTrackedHandler(sender, args) { removeMarkers(args); }
    function aircraftSelectionChangedHandler(sender, args) { refreshMarker(args.aircraft); };
    function optionsChangedHandler(sender, args) { applyOptions(); };
    function zoomChangedHandler(sender, args) { refreshPinImages(); }

    this.markerClickHandler = function(googlePin)
    {
        var aircraft = getAircraftForGooglePin(googlePin);
        if(aircraft !== null) mEvents.raise(EventId.aircraftMarkerClicked, this, aircraft);
    }

    function refreshMarkers(aircraftList)
    {
        // Calculate some date stuff once so we don't need to keep doing it for every aircraft
        var utcTicks = mOptions.tempServerTime.getTime();
        var shortTrailThresholdTicks = utcTicks - (1000 * mOptions.tempServerShortTrailLengthSeconds);
        shortTrailThresholdTicks -= 500; // just a little extra to fudge the fact that trail timestamps may not exactly align on 1 second boundaries

        var length = aircraftList.length;
        for(var i = 0;i < length;++i) {
            var aircraft = aircraftList[i];
            if(aircraft.getHasPos()) {
                var marker = aircraft.Marker;
                if(marker === undefined) marker = createMarker(aircraft);
                else marker.refresh();

                if(aircraft.Cot !== null && aircraft.Cot.length > 0) marker.setFullTrailPoints(aircraft.ResetTrail, aircraft.Cot);
                if(aircraft.Cos !== null && aircraft.Cos.length > 0) marker.setShortTrailPoints(aircraft.ResetTrail, aircraft.Cos, shortTrailThresholdTicks);
            }
        }
    }

    function refreshMarker(aircraft)
    {
        if(aircraft !== null) {
            var marker = aircraft.Marker;
            if(marker !== undefined) marker.applyOptions();
        }
    };

    function removeMarkers(aircraftList)
    {
        var length = aircraftList.length;
        for(var i = 0;i < length;++i) {
            var aircraft = aircraftList[i];
            var marker = aircraft.Marker;
            if(marker !== undefined) {
                aircraft.Marker = undefined;
                marker.makeDormant();
                mOldMarkers.push(marker);
            }
        }
    }

    function createMarker(aircraft)
    {
        var result = null;

        if(mOldMarkers.length == 0) result = new GoogleMapMarker(mOptions, mMap, googleMapMarkerCollection_MarkerClickHandler);
        else {
            result = mOldMarkers.pop();
            result.eraseTrace();
        }

        aircraft.Marker = result;
        result.initialiseForAircraft(aircraft);

        return result;
    };

    function refreshPinImages()
    {
        var allAircraft = mAircraftCollection.getAllAircraft();
        var length = allAircraft.length;
        for(var i = 0;i < length;++i) {
            var aircraft = allAircraft[i];
            var marker = aircraft.Marker;
            if(marker !== undefined) marker.refreshPinImage();
        }
    };

    function applyOptions()
    {
        var allAircraft = mAircraftCollection.getAllAircraft();
        var length = allAircraft.length;
        for(var i = 0;i < length;++i) {
            var aircraft = allAircraft[i];
            var marker = aircraft.Marker;
            if(marker !== undefined) marker.applyOptions();
        }
    };

    function getAircraftForGooglePin(googlePin)
    {
        var result = null;
        var marker = googlePin._VirtualRadarServerMarkerReference;
        if(marker !== undefined) result = marker.getAircraft();

        return result;
    };

    function getDebugCountPoints(sender, args)
    {
        var marker = args.aircraft.Marker;
        if(marker !== undefined) args.result = marker.getDebugCountPoints();
    };

    function getDebugPointText(sender, args)
    {
        var marker = args.aircraft.Marker;
        if(marker !== undefined) args.result = marker.getDebugPointText();
    };
}