function GoogleMapMovingMapControl(events, options)
{
    var that = this;
    var mEvents = events;
    var mContainerElement;
    var mInnerElement;
    var mIconElement;
    var mOptions = options;
    var mVisible = options.showMovingMap;

    var mChecked = options.movingMapChecked;
    this.getChecked = function() { return mChecked; };
    this.setChecked = function(value)
    {
        mChecked = value;
        refreshState();
        mOptions.movingMapChecked = value;
        mEvents.raise(EventId.optionsChanged, null, null);
    };

    mEvents.addListener(EventId.optionsChanged, optionsChangedHandler);
    mEvents.addListener(EventId.acListRefreshed, aircraftListRefreshedHandler);
    mEvents.addListener(EventId.acSelected, aircraftSelectedHandler);

    function optionsChangedHandler(sender, args)
    {
        if(mOptions.showMovingMap !== mVisible) {
            mVisible = mOptions.showMovingMap;
            if(!mVisible && mChecked) that.setChecked(false);
            refreshState();
        }
    };

    function aircraftListRefreshedHandler(sender, args) { onRefreshMovingMapPosition(null); };
    function aircraftSelectedHandler(sender, args)      { onRefreshMovingMapPosition(null); };
    function onRefreshMovingMapPosition(args)           { if(mVisible && mChecked) mEvents.raise(EventId.refreshMovingMapPosition, that, args); };

    this.addToMap = function(map)
    {
        // Pre-load the images
        createImage('Images/MovingMapChecked.png');
        var defaultImage = createImage('Images/MovingMapUnchecked.png', 15, 15, 'Toggle moving map');

        mContainerElement = createElement('div', 'googleMapButton movingMapControl');
        mInnerElement = createElement('div', 'movingMapInner', mContainerElement);
        mIconElement = createElement('div', 'movingmapIcon', mInnerElement);

        mIconElement.appendChild(defaultImage);

        google.maps.event.addDomListener(mIconElement, 'click', onToggleMovingMap);
        google.maps.event.addDomListener(mIconElement, 'doubleclick', onToggleMovingMap);

        map.controls[google.maps.ControlPosition.TOP_RIGHT].push(mContainerElement);

        refreshState();
    };

    function onToggleMovingMap()
    {
        that.setChecked(!mChecked);
        onRefreshMovingMapPosition(null);
    }

    function refreshState()
    {
        mContainerElement.style.display = mVisible ? 'block' : 'none';
        if(mIconElement !== undefined) {
            var imageText = mChecked ? 'Checked' : 'Unchecked';
            mIconElement.firstChild.setAttribute('src', 'Images/MovingMap' + imageText + '.png');
        }
    };
}