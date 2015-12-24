var _Report;
var _Events;

function initialise()
{
    _Events = new Events();
    _Report = new RegReport();

    _Report.initialise();
}

function RegReport()
{
    var that = this.registerSubclass(this);

    var mReportAircraftDetail = new ReportAircraftDetail();
    var mReportAircraftFlights = new ReportAircraftFlights();

    this.initialiseSubclass = function()
    {
        document.title = 'Virtual Radar - Registration Report';
        that.setCriteria(new RegReportCriteria());
        that.setMap(new ReportMap(''));
        that.getReportRowProvider().setReportId("reg");
    };

    this.subclassFillCriteria = function(params)
    {
        var result = false;
        var criteria = that.getCriteria();
        var reg = getPageParameterValue(params, "reg");
        if(reg !== null && reg.length > 0) {
            criteria.setRegistration(reg);
            result = true;
        }

        return result;
    };

    this.showSubclassContent = function(reportContent)
    {
        normaliseAircraft(reportContent.aircraftDetail);
        normaliseFlightList(reportContent.flights);

        document.title = 'Virtual Radar - ' + reportContent.aircraftDetail.reg;

        mReportAircraftDetail.showDetail(reportContent.aircraftDetail);
        mReportAircraftFlights.showFlights(reportContent);
    };
}

RegReport.prototype = new Report('');