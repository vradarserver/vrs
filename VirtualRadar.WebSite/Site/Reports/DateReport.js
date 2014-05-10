var _Report;
var _Events;

function initialise()
{
    _Events = new Events();
    _Report = new DateReport();

    _Report.initialise();
}

function DateReport()
{
    var that = this.registerSubclass(this);
    var mTitle = null;

    var mReportFlightsDetail = new ReportFlightsDetail();
    var mReportFlights = new ReportFlights();

    this.initialiseSubclass = function()
    {
        document.title = 'Virtual Radar - Date Report';
        that.setCriteria(new DateReportCriteria());
        that.setMap(new ReportMap('Fr'));
        that.getReportRowProvider().setReportId("date");
    };

    this.subclassFillCriteria = function(params)
    {
        var result = false;
        var criteria = that.getCriteria();

        var date = getPageParameterValue(params, "date");
        if(date !== null && date.length > 0) {
            if(date === 'today') {
                var today = new Date();
                date = today.getFullYear().toString() + '-' + (today.getMonth() + 1).toString() + '-' + today.getDate().toString();
            }
            criteria.setFromDate(date);
            criteria.setToDate(date);
            mTitle = date;
            result = true;
        }

        var callsign = getPageParameterValue(params, "callsign");
        if(callsign !== null && callsign.length > 0) {
            criteria.setCallsign(callsign);
            mTitle = callsign;
            result = true;
        }

        return result;
    };

    this.showSubclassContent = function(reportContent)
    {
        normaliseFlightList(reportContent.flights);
        normaliseAircraftList(reportContent.aircraftList);

        if(mTitle !== null) document.title = 'Virtual Radar - ' + mTitle;

        var criteria = that.getCriteria();
        if(reportContent.fromDate !== undefined && reportContent.fromDate !== null) criteria.setFromDate(reportContent.fromDate);
        if(reportContent.toDate !== undefined && reportContent.toDate !== null) criteria.setToDate(reportContent.toDate);
        criteria.copyToUI();

        mReportFlights.showFlights(reportContent);
    };

    this.showAllFlightsDetail = function(idx)
    {
        var reportContent = that.getReportContent();
        var flight = reportContent.flights[idx];
        var aircraft = flight.acIdx > -1 ? reportContent.aircraftList[flight.acIdx] : null;
        var route = reportBuildRoute(flight, reportContent.routes, reportContent.airports);

        mReportFlightsDetail.showFlightsDetail(flight, aircraft, route);
        that.displayPath(idx);
    };
}

DateReport.prototype = new Report('Fr');