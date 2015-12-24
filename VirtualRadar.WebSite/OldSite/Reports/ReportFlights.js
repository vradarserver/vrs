var ReportFlightsColumn = {
    RowNumber: 1,
    Date: 2,
    Time: 3,
    ICAO: 4,
    Reg: 5,
    Type: 6,
    Model: 7,
    Callsign: 8,
    From: 9,
    Via: 10,
    To: 11,
    Operator: 12,
    ModeSCountry: 13,
    Species: 14,
    Military: 15
};

function ReportFlights()
{
    var that = this;
    var mColumns = [];
    var mIsCallsignReport = false;

    function createColumns(groupBy)
    {
        mColumns = [];
        mColumns.push(ReportFlightsColumn.RowNumber);
        if(groupBy != 'date')       mColumns.push(ReportFlightsColumn.Date);
        mColumns.push(ReportFlightsColumn.Time);
        if(groupBy != 'icao')       mColumns.push(ReportFlightsColumn.ICAO);
        if(groupBy != 'reg')        mColumns.push(ReportFlightsColumn.Reg);
        if(groupBy != 'operator')   mColumns.push(ReportFlightsColumn.Operator);
        if(groupBy != 'type')       mColumns.push(ReportFlightsColumn.Type);
        if(groupBy != 'model')      mColumns.push(ReportFlightsColumn.Model);
        if(groupBy != 'type')       mColumns.push(ReportFlightsColumn.Species);
        if(groupBy != 'country')    mColumns.push(ReportFlightsColumn.ModeSCountry);
        mColumns.push(ReportFlightsColumn.Military);
        if(groupBy != 'callsign')   mColumns.push(ReportFlightsColumn.Callsign);
        mColumns.push(ReportFlightsColumn.From);
        mColumns.push(ReportFlightsColumn.Via);
        mColumns.push(ReportFlightsColumn.To);
    };

    this.showFlights = function(reportContent)
    {
        var flights = reportContent.flights;
        var aircraftList = reportContent.aircraftList;
        var airports = reportContent.airports;
        var routes = reportContent.routes;
        var groupBy = reportContent.groupBy;
        var operatorFlagsAvailable = reportContent.operatorFlagsAvailable;
        var silhouettesAvailable = reportContent.silhouettesAvailable;

        var html = ''

        var pageParams = getPageParameters();
        mIsCallsignReport = getPageParameterValue(pageParams, 'callsign') !== null;

        createColumns(groupBy);

        html += '<table class="allFlights">';
        html += '<tr class="allFlightsHeading">' + buildTableHeading() + '</tr>';

        var previousFlight = null, previousAircraft = null;
        for(var flightNum = 0;flightNum < flights.length;++flightNum) {
            var flight = flights[flightNum];
            var aircraft = flight.acIdx > -1 ? aircraftList[flight.acIdx] : null;
            var route = reportBuildRoute(flight, routes, airports);

            html += showGroupHeading(flight, previousFlight, aircraft, previousAircraft, groupBy, operatorFlagsAvailable, silhouettesAvailable);

            var classes = 'allFlightsRow ';
            if(flight.hEmg) classes += 'allFlightsEmg';
            else            classes += (flightNum % 2 == 0 ? 'allFlightsOdd' : 'allFlightsEven');
            html += '<tr class="' + classes + '" onclick="showAllFlightsDetail(' + flightNum + ')">' + buildTableRow(flight, aircraft, route, operatorFlagsAvailable, silhouettesAvailable) + '</tr>';

            previousFlight = flight;
            previousAircraft = aircraft;
        }

        html += '</table>';

        document.getElementById('flights').innerHTML = html;
    };

    function showGroupHeading(flight, previousFlight, aircraft, previousAircraft, groupBy, operatorFlagsAvailable, silhouettesAvailable)
    {
        var html = '';
        if(groupBy !== null && groupBy !== 'none') {
            var hasChanged = false;
            switch(groupBy) {
                case 'callsign':    hasChanged = previousFlight === null || flight.call !== previousFlight.call; break;
                case 'country':     hasChanged = previousAircraft === null || aircraft.modeSCountry !== previousAircraft.modeSCountry; break;
                case 'date':        hasChanged = previousFlight === null ||
                                                    (flight.start.getFullYear() !== previousFlight.start.getFullYear() ||
                                                    flight.start.getMonth() !== previousFlight.start.getMonth() ||
                                                    flight.start.getDate() !== previousFlight.start.getDate()); break;
                case 'icao':        hasChanged = previousAircraft === null || aircraft.icao !== previousAircraft.icao; break;
                case 'model':       hasChanged = previousAircraft === null || aircraft.typ !== previousAircraft.typ; break;
                case 'type':        hasChanged = previousAircraft === null || aircraft.icaoType !== previousAircraft.icaoType; break;
                case 'operator':    hasChanged = previousAircraft === null || aircraft.owner !== previousAircraft.owner; break;
                case 'reg':         hasChanged = previousAircraft === null || aircraft.reg !== previousAircraft.reg; break;
            }

            if(hasChanged) {
                var header = '', missingValue = 'None';
                switch(groupBy) {
                    case 'callsign':    header = mIsCallsignReport ? flight.call : formatCallsignReportLink(flight.call); break;
                    case 'country':     header = aircraft.modeSCountry; break;
                    case 'date':        header = formatDateHeading(flight.start); break;
                    case 'icao':        header = aircraft.icao; break;
                    case 'model':       header = aircraft.typ; break;
                    case 'type':
                        header = silhouettesAvailable ? formatSilhouette(aircraft) : '';
                        if(header.length > 0) header += '&nbsp;'
                        header += aircraft.icaoType;
                        var icao8643 = formatIcao8643Details(aircraft.engines, aircraft.engType, aircraft.species, aircraft.wtc);
                        if(icao8643 && icao8643.length > 0) {
                            if(!header || header.length === 0) header = icao8643;
                            else header = header + ':&nbsp;&nbsp;' + icao8643;
                        }
                        break;
                    case 'operator':
                        header = operatorFlagsAvailable ? formatOperatorFlag(aircraft) : '';
                        if(header.length > 0) header += '&nbsp;';
                        header += aircraft.owner;
                        break;
                    case 'reg':         header = aircraft.reg; break;
                }
                if(header !== null) header = trim(header);
                if(header === null || header.length === 0) header = missingValue;
                html = '<tr class="allFlightsGroup"><td></td><td colspan="' + (mColumns.length - 1) + '">';
                html += header + '</td></tr>';
            }
        }

        return html;
    };

    function buildTableHeading()
    {
        var result = '';

        var heading;
        for(var i = 0;i < mColumns.length;++i) {
            heading = '';
            switch(mColumns[i]) {
                case ReportFlightsColumn.Callsign:      heading = 'Callsign'; break;
                case ReportFlightsColumn.Date:          heading = 'Date'; break;
                case ReportFlightsColumn.From:          heading = 'From'; break;
                case ReportFlightsColumn.ICAO:          heading = 'ICAO'; break;
                case ReportFlightsColumn.Military:      heading = 'Military'; break;
                case ReportFlightsColumn.Model:         heading = 'Model'; break;
                case ReportFlightsColumn.ModeSCountry:  heading = 'Mode-S Country'; break;
                case ReportFlightsColumn.Operator:      heading = 'Operator'; break;
                case ReportFlightsColumn.Reg:           heading = 'Reg.'; break;
                case ReportFlightsColumn.RowNumber:     break;
                case ReportFlightsColumn.Species:       heading = 'Species'; break;
                case ReportFlightsColumn.Time:          heading = 'Time'; break;
                case ReportFlightsColumn.To:            heading = 'To'; break;
                case ReportFlightsColumn.Type:          heading = 'Type'; break;
                case ReportFlightsColumn.Via:           heading = 'Via'; break;
           }
           result += '<th>' + heading + '</th>';
        }

        return result;
    };

    function buildTableRow(flight, aircraft, route, operatorFlagsAvailable, silhouettesAvailable)
    {
        var result = ''
        var content;
        var tooltip;
        for(var i = 0;i < mColumns.length;++i) {
            content = '';
            tooltip = null;
            switch(mColumns[i]) {
                case ReportFlightsColumn.Callsign:      content = mIsCallsignReport ? flight.call : formatCallsignReportLink(flight.call); break;
                case ReportFlightsColumn.Date:          content = formatIsoDate(flight.start); break;
                case ReportFlightsColumn.From:          if(route != null && route.from != null) { content = route.from.code; tooltip = route.from.name; } break;
                case ReportFlightsColumn.ICAO:          content = formatIcaoReportLink(aircraft.icao); break;
                case ReportFlightsColumn.Military:      content = aircraft.military ? 'Yes' : 'No'; break;
                case ReportFlightsColumn.Model:         content = aircraft.typ; break;
                case ReportFlightsColumn.ModeSCountry:  content = aircraft.modeSCountry; break;
                case ReportFlightsColumn.Operator:      content = (operatorFlagsAvailable ? formatOperatorFlag(aircraft, true) + '&nbsp;' : '') + aircraft.owner; break;
                case ReportFlightsColumn.Species:       content = capitaliseSentence(formatSpecies(aircraft.species, true)); break;
                case ReportFlightsColumn.Reg:           content = formatRegReportLink(aircraft.reg); break;
                case ReportFlightsColumn.RowNumber:     content = flight.row; break;
                case ReportFlightsColumn.Time:          content = intToString(flight.start.getHours(), 2) + ':' + intToString(flight.start.getMinutes(), 2) + ':' + intToString(flight.start.getSeconds(), 2); break;
                case ReportFlightsColumn.To:            if(route != null && route.to != null) { content = route.to.code; tooltip = route.to.name; } break;
                case ReportFlightsColumn.Type:
                    content = (silhouettesAvailable ? formatSilhouette(aircraft, true) + '&nbsp;' : '') + aircraft.icaoType;
                    tooltip = capitaliseSentence(formatIcao8643Details(aircraft.engines, aircraft.engType, aircraft.species, aircraft.wtc));
                    break;
                case ReportFlightsColumn.Via:
                    if(route != null && route.via.length > 0) {
                        tooltip = '';
                        for(var c = 0;c < route.via.length;c++) {
                            if(c != 0) {
                                content += '-';
                                tooltip += ' - ';
                            }
                            content += route.via[c].code;
                            tooltip += route.via[c].name;
                        }
                    }
                    break;
            }

            result += '<td class="allFlightsCell"';
            if(tooltip != null) result += ' title="' + tooltip + '"';
            result += '>' + content + '</td>';
        }

        return result;
    };
}