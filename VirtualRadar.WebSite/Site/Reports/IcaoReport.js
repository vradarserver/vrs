var _Report;
var _Events;

function initialise()
{
    _Events = new Events();
    _Report = new IcaoReport();

    _Report.initialise();
}

function IcaoReport()
{
    var that = this.registerSubclass(this);

    var mReportAircraftDetail = new ReportAircraftDetail();
    var mReportAircraftFlights = new ReportAircraftFlights();

    this.initialiseSubclass = function()
    {
        document.title = 'Virtual Radar - ICAO Report';
        that.setCriteria(new IcaoReportCriteria());
        that.setMap(new ReportMap(''));
        that.getReportRowProvider().setReportId("icao");
    };

    this.subclassFillCriteria = function(params)
    {
        var result = false;
        var criteria = that.getCriteria();
        var icao = getPageParameterValue(params, "icao");
        if(icao !== null && icao.length > 0) {
            criteria.setIcao(icao);
            result = true;
        }

        return result;
    };

    this.showSubclassContent = function(reportContent)
    {
        normaliseAircraft(reportContent.aircraftDetail);
        normaliseFlightList(reportContent.flights);

        document.title = 'Virtual Radar - ' + reportContent.aircraftDetail.icao;

        mReportAircraftDetail.showDetail(reportContent.aircraftDetail);
        mReportAircraftFlights.showFlights(reportContent);
    };
}

IcaoReport.prototype = new Report('');