function ReportRowProvider()
{
    var that = this;
    var mXHR = new XHR();
    var mFetchListeners = [];
    var mReportId = "";
    var mCriteriaText = "";

    this.addFetchListener = function(address) { mFetchListeners.push(address); };
    this.getReportId = function() { return mReportId; };
    this.setReportId = function(value) { mReportId = value; };
    this.getCriteriaText = function() { return mCriteriaText; };
    this.setCriteriaText = function(value) { mCriteriaText = value; };

    function getAddress()
    {
        var timeNow = new Date();
        var result = "ReportRows.json?rep=" + mReportId;  // time parameter no longer required, server now sends cache-control header
        if(mCriteriaText !== null && mCriteriaText.length > 0) result += mCriteriaText;

        return result;
    };

    this.startFetchingRows = function()
    {
        mXHR.beginSend("get", getAddress(), null, null, 60000, fetchHandler);
    };

    function fetchHandler(status, responseText)
    {
        var response = null;
        if(status !== 200) response = { errorText: "Virtual Radar Server returned status " + status };
        else {
            responseText = replaceDateConstructors(responseText);
            response = eval('(' + responseText + ')');
        }
        for(var i = 0;i < mFetchListeners.length;++i) mFetchListeners[i](response);
   };
}