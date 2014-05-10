function ReportFlightsDetail()
{
    var that = this.registerSubclass(this);
    var mFlight = null;
    var mRoute = null;

    this.setShowLinksToAircraftReports(true);

    this.showFlightsDetail = function(flight, aircraft, route)
    {
        mFlight = flight;
        mRoute = route;
        that.showDetail(aircraft);
    };

    this.subclassAddToEndOfOtherTable = function()
    {
        var html = '';
        var duration = reportDuration(mFlight.start, mFlight.end);

        var pageParams = getPageParameters();
        var isCallsignReport = getPageParameterValue(pageParams, 'callsign') !== null;

        if(mFlight.start !== null) html += that.addOtherRow('Flight Detected', mFlight.start.toString());
        if(mFlight.end !== null) html += that.addOtherRow('Lost Contact', mFlight.end.toString());
        if(duration !== null) html += that.addOtherRow('Duration', duration);
        if(mFlight.call !== null && mFlight.call.length > 0) html += that.addOtherRow('Callsign', isCallsignReport ? mFlight.call : formatCallsignReportLink(mFlight.call));

        if(mRoute !== null) {
            if(mRoute.from !== null) html += that.addOtherRow('From', formatAirport(mRoute.from));
            if(mRoute.via !== null) {
                for(var i = 0;i < mRoute.via.length;++i) {
                    html += that.addOtherRow(i == 0 ? 'Via' : '', formatAirport(mRoute.via[i]));
                }
            }
            if(mRoute.to !== null) html += that.addOtherRow('To', formatAirport(mRoute.to));
        }

        if(mFlight.hEmg) html += that.addOtherRow('Emergency', 'Yes');
        if(mFlight.hAlrt) html += that.addOtherRow('Alert', 'Yes');
        if(mFlight.fAlt != 0 || mFlight.lAlt != 0) html += that.addOtherRow('Altitude', reportFirstAndLast(mFlight.fAlt, mFlight.lAlt, 0));
        if(mFlight.fSpd != 0 || mFlight.lSpd != 0) html += that.addOtherRow('Speed', reportFirstAndLast(mFlight.fSpd, mFlight.lSpd, 0));
        if(mFlight.fSqk != 0 || mFlight.lSqk != 0) html += that.addOtherRow('Squawk', reportFirstAndLast(intToString(mFlight.fSqk, 4), intToString(mFlight.lSqk, 4), "0000"));
        if(mFlight.end !== null) {
            html += that.addOtherRow('ADS-B Messages', mFlight.cADSB);
            html += that.addOtherRow('Mode-S Messages', mFlight.cMDS);
            html += that.addOtherRow('Position Messages', mFlight.cPOS);
        }

        return html;
    };

    function formatAirport(airport)
    {
        var result = airport.code === null ? '' : airport.code;
        if(airport.name !== null && airport.name.length > 0) {
            if(result.length > 0) result += ' ';
            result += airport.name;
        }

        return result;
    }
};

ReportFlightsDetail.prototype = new ReportAircraftDetail();