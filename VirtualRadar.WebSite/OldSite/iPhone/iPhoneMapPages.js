var PageId = {
    map: 0,
    aircraftList: 1,
    aircraftDetail: 2,
    options: 3,
    optionsRefreshPeriod: 4,
    optionsDistanceUnit: 5,
    optionsHeightUnit: 6,
    optionsSpeedUnit: 7,
    optionsPinText1: 8,
    optionsPinText2: 9,
    optionsPinText3: 10,
    optionsSortListColumn1: 11,
    optionsSortListColumn2: 12,
    optionsSortListDir1: 13,
    optionsSortListDir2: 14,
    optionsFilterWtc: 15,
    optionsFilterSpecies: 16,
    optionsFilterEngType: 17,
    optionsFlightLevelTransitionAltitudeUnit: 18,
    optionsFlightLevelHeightUnit: 19,
    _Last: 19
};

var PageAnimation = {
    none: 0
};

function iPhoneMapPages(events)
{
    var that = this;
    var mEvents = events;
    var mCssElements = [];
    var mSelectedId = 0;
    this.getSelectedPageId = function() { return mSelectedId; };

    var mPageIds = [
        'map_canvas',
        'plane_list',
        'aircraft_detail',
        'options',
        'optionsRefreshPeriod',
        'optionsDistanceUnit',
        'optionsHeightUnit',
        'optionsSpeedUnit',
        'optionsPinText1',
        'optionsPinText2',
        'optionsPinText3',
        'optionsSortListColumn1',
        'optionsSortListColumn2',
        'optionsSortListDir1',
        'optionsSortListDir2',
        'optionsFilterWtc',
        'optionsFilterSpecies',
        'optionsFilterEngType',
        'optionsFlightLevelTransitionAltitudeUnit',
        'optionsFlightLevelHeightUnit'
    ];

    this.initialise = function()
    {
        var stylesheetIndex = findStylesheet('.iPhoneMapStylesheet');

        for(var i = 0;i <= PageId._Last;++i) {
            mCssElements[i] = getCSSRule(stylesheetIndex, 'div#' + mPageIds[i]);
        }
    };

    this.getPageHtmlId = function(pageId)
    {
        return mPageIds[pageId];
    };

    this.select = function(pageId, animation)
    {
        if(mSelectedId !== pageId) {
            mEvents.raise(EventId.resetTimeOut, null, null);

            var oldPageCss = mCssElements[mSelectedId].style;
            var newPageCss = mCssElements[pageId].style;

            var screenWidth = window.innerWidth;
            var screenHeight = window.innerHeight;

            // Deal with fancy animations later :)
            oldPageCss.display = 'none';
            newPageCss.display = 'block';

            window.scrollTo(0, 1);

            mSelectedId = pageId;
            mEvents.raise(EventId.iPhonePageSwitched, this, mSelectedId);
        }
    };
}