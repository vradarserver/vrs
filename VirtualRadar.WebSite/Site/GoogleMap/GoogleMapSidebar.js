function GoogleMapSidebar(events)
{
    var that = this;

    var mInitialWidth = 390;
    var mMaxWidth = 600;
    var mMargin = 10;
    var mScrollbarMargin = 20;
    var mWidth = mInitialWidth;
    var mVisible = true;
    var mEvents = events;

    this.getInitialWidth = function() { return mInitialWidth; };
    this.getWidth = function() { return mWidth; };
    this.setWidth = function(value) { mWidth = Math.min(mMaxWidth, Math.max(mInitialWidth, value)); configureCss(); };
    this.getVisible = function() { return mVisible; };
    this.setVisible = function(value) { mVisible = value; configureCss(); };

    function getMapCanvas(stylesheetIndex) { return getCSSRule(stylesheetIndex, 'div#map_canvas'); };
    function getSplitter(stylesheetIndex) { return getCSSRule(stylesheetIndex, 'div#splitter'); };
    function getSidebarCanvas(stylesheetIndex) { return getCSSRule(stylesheetIndex, 'div#sidebar_canvas'); };
    function getSidebarContainer(stylesheetIndex) { return getCSSRule(stylesheetIndex, 'div#sidebarContainer'); };
    function getMapMargin() { return mWidth + mScrollbarMargin + mMargin; }

    this.modifyUI = function()
    {
        var splitterDiv = document.getElementById('splitter');
        splitterDiv.onmouseover = onMouseOver;
        splitterDiv.onmouseout = onMouseOut;
        splitterDiv.onclick = onClick;

        configureCss();
    };

    function onMouseOver()  { highlightSplitter(true); };
    function onMouseOut()   { highlightSplitter(false); };
    function onClick()
    {
        that.setVisible(!that.getVisible());
        onMouseOut();
        mEvents.raise(EventId.resetTimeOut, null, null);
    }

    function configureCss()
    {
        var stylesheetIndex = findStylesheet('.googleMapStylesheet');
        var mapCanvas = getMapCanvas(stylesheetIndex);
        var splitter = getSplitter(stylesheetIndex);
        var sidebarCanvas = getSidebarCanvas(stylesheetIndex);
        var sidebarContainer = getSidebarContainer(stylesheetIndex);

        if(!mVisible) {
            mapCanvas.style.marginRight = mMargin.toString() + 'px';
            splitter.style.marginLeft = '-' + mMargin.toString() + 'px';
            splitter.style.backgroundImage = 'url("Images/ShowList.png")';
            sidebarCanvas.style.display = 'none';
        } else {
            var mapMargin = getMapMargin();

            mapCanvas.style.marginRight = mapMargin.toString() + 'px';

            splitter.style.marginLeft = '-' + mapMargin.toString() + 'px';
            splitter.style.backgroundImage = 'url("Images/HideList.png")';

            sidebarCanvas.style.display = 'block';
            sidebarCanvas.style.width = (mWidth + mScrollbarMargin).toString() + 'px';
            sidebarCanvas.style.marginLeft = '-' + (mWidth + mScrollbarMargin) + 'px';

            sidebarContainer.style.width = mWidth.toString() + 'px';
        }

        mEvents.raise(EventId.sidebarResized, null, null);
    };

    function highlightSplitter(highlight)
    {
        var splitter = getSplitter(findStylesheet('.googleMapStylesheet'));
        splitter.style.backgroundColor = highlight ? '#f0f0f0' : '#ffffff';
    };
};