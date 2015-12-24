var EventId = {
    resetTimeOut: 0,
    setAutoSelect: 1,
    acDeselected: 2,
    acSelected: 3,
    acAutoSelected: 4,
    newAircraft: 5,
    acListRefreshed: 6,
    noLongerTracked: 7,
    aircraftDetailRefresh: 8,
    selectClosestClicked: 9,
    toggleAutoSelectClicked: 10,
    listChooseColumnsChanged: 11,
    currentLocationChanged: 12,
    optionsChanged: 13,
    optionsResetMapClicked: 14,
    autoSelectChanged: 15,
    optionsUICreateTabPage: 16,
    reverseGeocodeResponseChanged: 17,
    sidebarResized: 18,
    siteTimedOut: 19,
    userChangedVolume: 20,
    debugGetCountPoints: 21,
    debugGetPointText: 22,
    displayFlightPath: 23,
    runReport: 24,
    refreshMovingMapPosition: 25,
    toggleReverseGeocode: 26,
    orientationChanged: 27,
    infoButtonClicked: 28,
    iPhonePageSwitched: 29,
    aircraftMarkerClicked: 30,
    geolocationUpdated: 31,
    gotoCurrentLocationClicked: 32,
    zoomChanged: 33,
    togglePauseClicked: 34,
    pauseChanged: 35
};

function Events()
{
    var that = this;
    var mEvents = [];

    function getEventListeners(id)
    {
        while(id >= mEvents.length) mEvents.push([]);
        return mEvents[id];
    }

    this.addListener = function(id, handler)
    {
        if(id === undefined) throw 'Event ID must be defined before it can be listened to';
        var listeners = getEventListeners(id);
        var length = listeners.length;
        var alreadyListening = false;
        for(var i = 0;i < length;i++) {
            if(listeners[i] === handler) {
                alreadyListening = true;
                break;
            }
        }

        if(!alreadyListening) listeners.push(handler);
    }

    this.raise = function(id, sender, args)
    {
        if(id === undefined) throw 'Event ID must be defined before it can be raised';
        var listeners = getEventListeners(id);
        var length = listeners.length;
        for(var i = 0;i < length;i++) {
            listeners[i](sender, args);
        }
    }
}