function GoogleMapTimeout(options, events)
{
    var that = this;
    var mOptions = options;
    var mEvents = events;
    var mIntervalMilliseconds = 12000;
    var mIntervalFraction = 0.2;

    var mActive = !_ServerConfig.isLocalAddress && _ServerConfig.internetClientTimeoutMinutes > 0;
    this.isActive = function() { return mActive; };
    var mTimeout = _ServerConfig.internetClientTimeoutMinutes;
    var mCountMinutes = 0;
    var mTimedOut = false;
    this.isTimedOut = function() { return mTimedOut; };

    if(mActive) {
        addToUI();
        setTimeout(checkTimeout, mIntervalMilliseconds);
        mEvents.addListener(EventId.resetTimeOut, resetTimeOut);
    }

    function addToUI()
    {
        var message = createElement('p', null, document.getElementById('timeout'));
        message.onclick = function() { window.location.reload(true); };
        message.innerHTML =
            'As a measure to save bandwidth this site has timed out because there has been no activity for the last ' + mTimeout +
            ' minute' + (mTimeout === 1 ? '' : 's') + '. Click this message to reload the site if you want to continue viewing the' +
            ' aircraft movements.';
    };

    function resetTimeOut(sender, args)
    {
        mCountMinutes = 0;
    };

    function checkTimeout()
    {
        if(mTimeout > 0) {
            mCountMinutes += mIntervalFraction;
            if(mCountMinutes >= mTimeout) stopSite();
            else setTimeout(checkTimeout, mIntervalMilliseconds);
        }
    };

    function stopSite()
    {
        mTimedOut = true;
        document.getElementById('timeout').style.display = 'block';
        mEvents.raise(EventId.siteTimedOut, null, null);
    };
}