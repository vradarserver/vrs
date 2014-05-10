// An object that wraps a reusable XMLHttpRequest object for us.
function XHR()
{
    var that = this;
    var mXhr = null;
    var mTimeSent = null;
    var mPeekInterval = 50;
    var mTimeoutAt = 0;
    var mInFlight = false;
    var mCallback;
    var mTimer = null;
    var mTimeoutCallback = null;

    this.getTimeSent = function() { return mTimeSent; };
    this.setTimeoutCallback = function(value) { mTimeoutCallback = value; };

    function createXhr()
    {
        var result = null;

        try {
            result = new XMLHttpRequest();
        } catch(ex) {
            try {
                result = new ActiveXObject("MSXML2.XMLHTTP.3.0");
            } catch(ex) {
                try {
                    result = new ActiveXObject("Msxml2.XMLHTTP");
                } catch (ex) {
                    try {
                        result = new ActiveXObject("Microsoft.XMLHTTP");
                    } catch (ex) {
                        result = null;
                    }
                }
            }
        }

        return result;
    };

    // Sends a request to the URL specified and calls callback with all responses.
    // If the request isn't honoured within timeout milliseconds then the request is
    // cancelled. Any outstanding request is cancelled.
    this.beginSend = function(requestType, url, userName, password, timeout, callback, postContent, headers)
    {
        if(userName === "") userName = null;
        if(password === "") password = null;

        if(mXhr === null) mXhr = createXhr();
        that.abort();

        if(mXhr !== null) {
            mCallback = callback;
            mTimeSent = new Date();
            mTimeoutAt = mTimeSent.getTime() + timeout;

            mXhr.open(requestType, url, true, userName, password);
            if(headers && headers.length > 0) {
                for(var i = 0;i < headers.length;++i) {
                    var header = headers[i];
                    mXhr.setRequestHeader(header.name, header.value);
                }
            }
            mXhr.send(postContent === undefined ? null : postContent);

            mInFlight = true;
            mTimer = setTimeout(checkStatus, mPeekInterval);
        }
    };

    // Aborts the last request.
    this.abort = function()
    {
        mInFlight = false;
        if(mTimer !== null) {
            clearTimeout(mTimer);
            mTimer = null;
        }
    };

    // Called on a timer to check the status of the request and see if it's been completed yet. Also checks to see
    // if the timeout has expired.
    function checkStatus()
    {
        mTimer = null;
        if(mInFlight) {
            if(mXhr.readyState === 4) {
                mInFlight = false;
                if(mCallback !== null) mCallback(mXhr.status, mXhr.responseText);
            } else {
                var timeNow = new Date();
                if(timeNow.getTime() <= mTimeoutAt) mTimer = setTimeout(checkStatus, mPeekInterval);
                else {
                    mInFlight = false;
                    if(mTimeoutCallback !== null) mTimeoutCallback(that);
                }
            }
        }
    };
}