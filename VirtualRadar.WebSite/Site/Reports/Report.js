var _CookieExpiryDays = 365;            // Number of days before the cookies expire

function showHideCriteria()         { _Report.showHideCriteria(); }
function runReportClicked()         { if(_Report !== undefined) _Report.runFreshReport(); return false; }
function displayPath(flightIndex)   { _Report.displayPath(flightIndex); }
function togglePageControlOptions() { _Report.togglePageControlOptions(); }
function goFirstPage()              { _Report.goFirstPage(); }
function goLastPage()               { _Report.goLastPage(); }
function goNextPage()               { _Report.goNextPage(); }
function goPreviousPage()           { _Report.goPreviousPage(); }
function goPage(pageNumber)         { _Report.goPage(pageNumber); }
function jumpToPage()               { return _Report.jumpToPage(); }
function saveRowsPerPage(elNum)     { _Report.saveRowsPerPage(elNum); }
function showAllRows()              { _Report.showAllRows(); }
function showAllFlightsDetail(idx)  { _Report.showAllFlightsDetail(idx); }

function Report(cookieSuffix)
{
    var that = this;
    this.registerSubclass = function(value) { that = value; return value; };

    var mReportPageControl = new ReportPageControl(cookieSuffix);
    var mReportRowProvider = new ReportRowProvider();
    var mReportMap = null;
    var mCriteria = null;
    var mReportContent = null;

    this.getCriteria = function() { return mCriteria; };
    this.setCriteria = function(value) { mCriteria = value; };
    this.getMap = function() { return mReportMap; };
    this.setMap = function(value) { mReportMap = value; };
    this.getReportRowProvider = function() { return mReportRowProvider; };
    this.getReportContent = function() { return mReportContent; };

    this.initialise = function() {
        that.initialiseSubclass();

        if(mReportMap !== null) mReportMap.addUI();
        mCriteria.addUI();
        mReportPageControl.addUI();

        mReportRowProvider.addFetchListener(rowsReceivedHandler);

        _Events.addListener(EventId.runReport, runReportHandler);

        if(fillInitialCriteria()) this.runReport();
        else this.showHideCriteria();
    };

    function runReportHandler()
    {
        that.runReport();
    };

    function fillInitialCriteria()
    {
        var result = false;
        var params = getPageParameters();
        result = that.subclassFillCriteria(params);

        mCriteria.copyToUI();

        return result;
    };

    this.runReport = function()
    {
        mCriteria.copyFromUI();
        if(mCriteria.isValid()) {
            showWaitAnimation();
            mReportRowProvider.setCriteriaText(mCriteria.toString() + mReportPageControl.addToCriteria());
            mReportRowProvider.startFetchingRows();
        }
    };

    this.runFreshReport = function()
    {
        mReportPageControl.setTotalRows(0);
        mReportPageControl.setFirstRow(0);
        mReportPageControl.refreshUI();

        that.runReport();
    };

    function rowsReceivedHandler(reportContent)
    {
        hideWaitAnimation();
        mReportContent = reportContent;
        mReportPageControl.setTotalRows(reportContent.countRows === undefined ? 0 : reportContent.countRows);
        mReportPageControl.refreshUI();

        var rowCount = null;
        if(reportContent.errorText !== undefined) {
            document.getElementById('errorText').innerHTML = reportContent.errorText;
            document.getElementById('errorText').style.display = 'block';
            document.getElementById('reportContent').style.display = 'none';
        } else {
            document.getElementById('errorText').style.display = 'none';
            document.getElementById('reportContent').style.display = 'block';
            that.showSubclassContent(reportContent);
            rowCount = reportContent.countRows !== undefined ? reportContent.countRows : null;
        }
        if(mCriteria !== null && mCriteria.setRowCount !== undefined) mCriteria.setRowCount(rowCount);
        if(reportContent.processingTime !== undefined) {
            document.getElementById('reportStatistics').innerHTML = 'Report produced in ' + reportContent.processingTime + ' seconds';
        }
    };

    function showWaitAnimation()
    {
        document.getElementById('waitAnimation').style.display = 'block';
        document.getElementById('nonStaticContent').style.display = 'none';
        document.getElementById('reportStatistics').style.display = 'none';
    };

    function hideWaitAnimation()
    {
        document.getElementById('waitAnimation').style.display = 'none';
        document.getElementById('nonStaticContent').style.display = 'block';
        document.getElementById('reportStatistics').style.display = 'block';
    };

    this.displayPath = function(flightIndex)
    {
        if(mReportContent !== null && mReportContent.flights.length > flightIndex) {
            _Events.raise(EventId.displayFlightPath, null, { flight: mReportContent.flights[flightIndex], index: flightIndex, count: mReportContent.flights.length });
        }
    };

    this.showHideCriteria = function()          { mCriteria.showHideCriteria(); };
    this.togglePageControlOptions = function()  { mReportPageControl.togglePageControlOptions(); };
    this.goFirstPage = function()               { mReportPageControl.goFirstPage(); };
    this.goLastPage = function()                { mReportPageControl.goLastPage(); };
    this.goNextPage = function()                { mReportPageControl.goNextPage(); };
    this.goPreviousPage = function()            { mReportPageControl.goPreviousPage(); };
    this.goPage = function(pageNumber)          { mReportPageControl.goPage(pageNumber); };
    this.jumpToPage = function()                { return mReportPageControl.jumpToPage(); };
    this.saveRowsPerPage = function(elNum)      { mReportPageControl.saveRowsPerPage(elNum); };
    this.showAllRows = function()               { mReportPageControl.showAllRows(); };
};

function normaliseAircraftList(aircraftList)
{
    var length = aircraftList.length;
    for(var i = 0;i < length;++i) {
        normaliseAircraft(aircraftList[i]);
    }
};

function normaliseAircraft(aircraft)
{
    if(aircraft.isUnknown === undefined || aircraft.isUnknown === false) {
        if(aircraft.reg === undefined) aircraft.reg = '';
        if(aircraft.icao === undefined) aircraft.icao = '';
        if(aircraft.opFlag === undefined) aircraft.opFlag = '';
        if(aircraft.owner === undefined) aircraft.owner = '';
        if(aircraft.typ === undefined) aircraft.typ = '';
        if(aircraft.popularName === undefined) aircraft.popularName = '';
        if(aircraft.icaoType === undefined) aircraft.icaoType = '';
    }
};

function normaliseFlightList(flightList)
{
    var length = flightList.length;
    for(var i = 0;i < length;++i) {
        normaliseFlight(flightList[i]);
    }
};

function normaliseFlight(flight)
{
    if(flight.start === undefined) flight.start = null;
    if(flight.end === undefined) flight.end = null;
    if(flight.call === undefined) flight.call = null;
    if(flight.hEmg === undefined) flight.hEmg = false;
    if(flight.hAlrt === undefined) flight.hAlrt = false;
    if(flight.fAlt === undefined) flight.fAlt = 0;
    if(flight.lAlt === undefined) flight.lAlt = 0;
    if(flight.fSpd === undefined) flight.fSpd = 0;
    if(flight.lSpd === undefined) flight.lSpd = 0;
    if(flight.fSqk === undefined) flight.fSqk = 0;
    if(flight.lSqk === undefined) flight.lSqk = 0;
    if(flight.fVsi === undefined) flight.fVsi = 0;
    if(flight.lVsi === undefined) flight.lVsi = 0;
};

function formatReportLink(page, description, critName, critValue, targetName)
{
    var result = !description ? '' : description;
    if(description && critValue) {
        var forceFrame = getPageParameterValue(null, 'forceFrame');
        forceFrame = forceFrame ? '&forceFrame=' + encodeURIComponent(forceFrame) : '';
        result = '<a href="' + page + '?' + critName + '=' + encodeURIComponent(critValue);
        if(critName === 'date-L') result += '&date-U=' + encodeURIComponent(critValue);
        result += forceFrame + '"' + target(targetName) + '>' + description + '</a>';
    }

    return result;
};

function formatRegReportLink(reg)           { return formatReportLink('RegReport.htm', reg, 'reg', reg, 'regReport'); };
function formatIcaoReportLink(icao)         { return formatReportLink('IcaoReport.htm', icao, 'icao', icao, 'icaoReport'); };
function formatCallsignReportLink(callsign) { return formatReportLink('DateReport.htm', callsign, 'callsign', callsign, 'callsignReport'); };
function formatDateReportLink(text, date)   { return formatReportLink('DateReport.htm', text, 'date', date ? formatIsoDate(date) : null, 'dateReport'); };

function formatIsoDate(date)
{
    return date === undefined || date === null ? '' : date.getFullYear().toString() + '-' + intToString(date.getMonth() + 1, 2) + '-' + intToString(date.getDate(), 2);
};

function formatDateHeading(date)
{
    return date === undefined || date === null ? '' : dayOfWeek(date.getDay(), true) + ' ' + date.getDate().toString() + ' ' + monthOfYear(date.getMonth(), true) + ' ' + date.getFullYear();
};

function formatTime(date)
{
    return date === undefined || date === null ? '' : intToString(date.getHours(), 2) + ':' + intToString(date.getMinutes(), 2) + ':' + intToString(date.getSeconds(), 2);
};

function reportBuildRoute(flight, routes, airports)
{
    var result = null;

    if(flight.rtIdx > -1) {
        var route = routes[flight.rtIdx];
        var from = route.fIdx > -1 ? airports[route.fIdx] : null;
        var to = route.tIdx > -1 ? airports[route.tIdx] : null;
        var via = [];
        for(var c = 0;c < route.sIdx.length;++c) {
            var idx = route.sIdx[c];
            var viaAirport = idx > -1 ? airports[idx] : null;
            if(viaAirport !== null) via.push(viaAirport);
        }

        result = { from: from, to: to, via: via };
    }

    return result;
};

function reportDuration(start, end)
{
    var result = null;

    if(start !== undefined && start !== null && end !== undefined && end !== null) {
        var span = getTimeSpan(start, end);
        var units = null;
        if(start === end || end < start) {
            result = 0;
            units = 'second';
        } else if(span.hours > 0) {
            result = span.hours;
            units = 'hour';
        } else if(span.minutes > 0) {
            result = span.minutes;
            units = 'minute';
        } else if(span.seconds > 0) {
            result = span.seconds;
            units = 'second';
        } else {
            result = span.milliseconds;
            units = 'millisecond';
        }
        if(result != 1) units += 's';
        result = result.toString() + ' ' + units;
    }

    return result;
};

function reportFirstAndLast(first, last, emptyValue)
{
    var result = '';
    if(first !== undefined && first !== emptyValue) result += first.toString();
    if(last !== undefined && last !== emptyValue && (first === undefined || last !== first)) {
        if(result.length > 0) result += " to ";
        result += last.toString();
    }

    return result;
};

function formatPicture(aircraft, extraTags, imgClass, linkToOriginal, size, altWidth, altHeight, forceBlankPicture)
{
    var result = '';
    if(size) {
        var validPicture = false;
        var filePortion = 'Images/File-' + encodeURIComponent(aircraft.reg ? aircraft.reg : '') + ' ' + encodeURIComponent(aircraft.icao);
        if(!forceBlankPicture && aircraft.hasPic && aircraft.icao) {
            result = '<img src="' + filePortion + '/Size-' + size + '/Picture.png"';
            validPicture = true;
        } else if(altWidth && altHeight) {
            result = '<img src="Images/Wdth-' + altWidth + '/Hght-' + altHeight + '/Blank.png" ';
        }
        if(result !== '') {
            if(altWidth) result += 'width="' + altWidth + 'px" ';
            if(altHeight) result += 'height="' + altHeight + 'px" ';
            if(imgClass) result += 'class="' + imgClass + '" ';
            if(extraTags) result += extraTags;
            result += ' />';

            if(linkToOriginal && validPicture) {
                result = '<a href="' + filePortion + '/Size-Full/Picture.png" target="picture">' + result + '</a>';
            }
        }
    }

    return result;
};

function formatOperatorFlag(aircraft, showBlankIfMissing)
{
    var result = aircraft && aircraft.opFlag && aircraft.opFlag.length > 0 ? '<img src="Images/File-' + aircraft.opFlag + '/OpFlag.png">' : '';
    if(showBlankIfMissing && result.length === 0) result = '<img src="Images/File-Blank/OpFlag.png">';

    return result;
};

function formatSilhouette(aircraft, showBlankIfMissing)
{
    var result = aircraft && aircraft.icaoType && aircraft.icaoType.length > 0 ? '<img src="Images/File-' + aircraft.icaoType + '/Type.png">' : '';
    if(showBlankIfMissing && result.length === 0) result = '<img src="Images/File-Blank/OpFlag.png">';

    return result;
};