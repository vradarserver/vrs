function GoogleMapInfoButton(events, options)
{
    var that = this;
    var mEvents = events;
    var mOptions = options;
    var mPaused = false;
    var mIcon;

    mEvents.addListener(EventId.optionsChanged, optionsChangedHandler);
    mEvents.addListener(EventId.pauseChanged, pauseChangedHandler);

    function optionsChangedHandler(sender, args)
    {
        setIconImage();
    };

    function pauseChangedHandler(sender, args)
    {
        mPaused = args;
        setIconImage();
    };

    this.addToMap = function(map)
    {
        var container = createElement('div', 'mapInfoButton');
        var inner = createElement('div', 'mapInfoButtonInner', container);
        mIcon = createElement('div', 'mapInfoButtonIcon', inner);
        setIconImage();

        google.maps.event.addDomListener(mIcon, 'click', onButtonClicked);
        google.maps.event.addDomListener(mIcon, 'doubleclick', onButtonClicked);

        map.controls[google.maps.ControlPosition.TOP_RIGHT].push(container);
    };

    function setIconImage()
    {
        if(mIcon) {
            var image = mPaused ? 'ChevronRedCircle' : mOptions.filteringEnabled ? 'ChevronBlueCircle' : 'ChevronGreenCircle';
            mIcon.innerHTML = '<img src="Images/' + image + '.png" width="26px" height="26px">';
        }
    };

    function onButtonClicked()
    {
        mEvents.raise(EventId.infoButtonClicked, this, null);
    };
}