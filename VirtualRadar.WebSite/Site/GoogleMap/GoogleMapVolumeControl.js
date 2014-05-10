function GoogleMapVolumeControl(events)
{
    var that = this;
    var mEvents = events;
    var mContainerElement;
    var mInnerElement;
    var mVolumeElement;
    var mVolumeUpElement;
    var mVolumeDownElement;

    var mVolume = 0;
    this.getVolume = function() { return mVolume; };
    this.setVolume = function(value)
    {
        var normalised = Math.max(0, Math.min(100, value));
        if(normalised !== mVolume) {
            mVolume = normalised;
            refreshState();
        }
    }

    var mMuted = false;
    this.getMuted = function() { return mMuted; };
    this.setMuted = function(value)
    {
        if(value !== mMuted) {
            mMuted = value;
            refreshState();
        }
    }

    this.addToMap = function(map)
    {
        // Pre-load the images
        var initialVolumeImage = createImage('Images/VolumeMute.png', 15, 15, 'Toggle mute');
        createImage('Images/Volume0.png');
        createImage('Images/Volume25.png');
        createImage('Images/Volume50.png');
        createImage('Images/Volume75.png');
        createImage('Images/Volume100.png');
        createImage('Images/VolumeDown.png');
        createImage('Images/VolumeUp.png');

        mContainerElement = createElement('div', 'googleMapButton volumeControl');
        mInnerElement = createElement('div', 'volumeInner', mContainerElement);
        mVolumeDownElement = createElement('div', 'volumeDown', mInnerElement);
        mVolumeUpElement = createElement('div', 'volumeUp', mInnerElement);
        mVolumeElement = createElement('div', 'volumeValue', mInnerElement);

        mVolumeElement.appendChild(initialVolumeImage);
        mVolumeDownElement.innerHTML = '<img src="Images/VolumeDown.png" width="15px" height="15px" alt="" title="Volume down">';
        mVolumeUpElement.innerHTML = '<img src="Images/VolumeUp.png" width="15px" height="15px" alt="" title="Volume up">';

        google.maps.event.addDomListener(mVolumeUpElement, 'click', onVolumeUp);
        google.maps.event.addDomListener(mVolumeUpElement, 'doubleclick', onVolumeUp);
        google.maps.event.addDomListener(mVolumeDownElement, 'click', onVolumeDown);
        google.maps.event.addDomListener(mVolumeDownElement, 'doubleclick', onVolumeDown);
        google.maps.event.addDomListener(mVolumeElement, 'click', onToggleMute);
        google.maps.event.addDomListener(mVolumeElement, 'doubleclick', onToggleMute);

        map.controls[google.maps.ControlPosition.TOP_RIGHT].push(mContainerElement);

        refreshState();
    }

    function onVolumeUp()   { that.setMuted(false); that.setVolume(mVolume + 25); mEvents.raise(EventId.userChangedVolume, null, null); }
    function onVolumeDown() { that.setMuted(false); that.setVolume(mVolume - 25); mEvents.raise(EventId.userChangedVolume, null, null); }
    function onToggleMute() { that.setMuted(!mMuted); mEvents.raise(EventId.userChangedVolume, null, null); }

    function refreshState()
    {
        if(mVolumeElement !== undefined) {
            var imageText = '';
            if(mMuted) imageText = 'Mute';
            else {
                if(mVolume == 0) imageText = '0';
                else if(mVolume <= 25) imageText = '25';
                else if(mVolume <= 50) imageText = '50';
                else if(mVolume <= 75) imageText = '75';
                else imageText = '100';
            }
            mVolumeElement.firstChild.setAttribute('src', 'Images/Volume' + imageText + '.png');
        }
    }
}