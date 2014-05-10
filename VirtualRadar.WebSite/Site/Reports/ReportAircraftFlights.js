function ReportAircraftFlights()
{
    var that = this;

    _Events.addListener(EventId.displayFlightPath, displayFlightPathHandler);

    this.showFlights = function(reportContent)
    {
        var flights = reportContent.flights;
        var airports = reportContent.airports;
        var routes = reportContent.routes;
        var groupBy = reportContent.groupBy;

        var html = '';
        var documentElement = document.getElementById('flights');
        documentElement.innerHTML = "";

        var previousGroupValue = null;
        for(var flightNum = 0;flightNum < flights.length;++flightNum) {
            if(flightNum + 1 % 5 == 0) {
                documentElement.innerHTML += html;
                html = "";
            }

            var flight = flights[flightNum];

            var isFirstInGroup = false;
            if(groupBy !== undefined && groupBy !== null) {
                switch(groupBy) {
                    case 'date':
                        isFirstInGroup = flightNum === 0;
                        if(!isFirstInGroup && previousGroupValue !== null && flight.start !== null) {
                            isFirstInGroup = flight.start.getFullYear() !== previousGroupValue.getFullYear() ||
                                             flight.start.getMonth() !== previousGroupValue.getMonth() ||
                                             flight.start.getDate() !== previousGroupValue.getDate();
                        }
                        previousGroupValue = flight.start;
                        break;
                    case 'callsign':
                        isFirstInGroup = flightNum === 0 || flight.call !== previousGroupValue;
                        previousGroupValue = flight.call;
                        break;
                }
            }

            var isEmergency = flight.hEmg;
            var canShowOnMap = flight.fLat !== undefined && flight.fLng !== undefined && flight.lLat !== undefined && flight.lLng !== undefined;
            var extraHeadingClasses = '';
            if(isEmergency) extraHeadingClasses = ' flightEmergency';
            else if(isFirstInGroup) extraHeadingClasses = ' flightFirstGroup';

            html += '<div id="flight' + flightNum.toString() + '" class="flight">' +
                    '<div class="flightBox3"><div class="flightBox4">';

            // Flight heading
            html += '<div class="flightHeading' + extraHeadingClasses + '">';

            html += '<div class="flightBox1"><div class="flightBox2">';
            html += '<table class="flightHeadingRow"><tr>';

            var startDate = '', startTime = '';
            if(flight.start === null) {
                startDate = 'Missing start time';
            } else {
                startDate = formatDateReportLink(formatDateHeading(flight.start), flight.start);
                startTime = formatTime(flight.start);
            }
            html += '<td class="flightDate' + extraHeadingClasses + '">' + startDate + '</td>' +
                    '<td class="flightCallsign' + extraHeadingClasses + '">' + formatCallsignReportLink(flight.call) + '</td>' +
                    '<td class="flightTime' + extraHeadingClasses + '">' + startTime + '</td>';

            html += '</tr></table>';
            html += '</div></div>'; // boxes
            html += '</div>'; // heading

            // Flight details
            html += '<div class="flightContent">';
            html += '<table class="flightDetail">';
            if(canShowOnMap) html += flightDetailLineNoPrint('Path', '<a href="#" onclick="displayPath(' + flightNum + ')">Show path taken on map</a>');
            var route = reportBuildRoute(flight, routes, airports);
            if(route !== null) {
                if(route.from !== null) html += flightRouteLine('From', route.from);
                if(route.via.length > 0) {
                    for(var i = 0;i < route.via.length;++i) {
                        html += flightRouteLine(i === 0 ? 'Via' : "", route.via[i]);
                    }
                }
                if(route.to !== null) html += flightRouteLine('To', route.to);
            }
            if(flight.hEmg) html += flightDetailLine('Emergency', 'Yes');
            if(flight.hAlrt) html += flightDetailLine('Alert', 'Yes');
            if(flight.fAlt != 0 || flight.lAlt != 0) html += flightFirstLastLine('Altitude', flight.fAlt, flight.lAlt, 0);
            if(flight.fSpd != 0 || flight.lSpd != 0) html += flightFirstLastLine('Speed', flight.fSpd, flight.lSpd, 0);
            if(flight.fSqk != 0 || flight.lSqk != 0) html += flightFirstLastLine('Squawk', intToString(flight.fSqk, 4), intToString(flight.lSqk, 4), "0000");
            var duration = reportDuration(flight.start, flight.end);
            if(duration !== null) html += flightDetailLine('Duration', duration);
            html += '</table>';

            html += '</div>';  // flightContent
            html += '</div></div>'; // bottom flightBoxes
            html += '</div>'; // flight
        }

        documentElement.innerHTML += html;
    };

    function flightDetailLine(name, value)
    {
        var result = '<tr><td class="flightDetailHeading">' + name;
        if(name !== null && name.length > 0) result += ':';
        return result + '</td><td class="flightDetailValue">' + value + '</td></tr>';
    };

    function flightDetailLineNoPrint(name, value)
    {
        var result = '<tr class="hideOnPrint"><td class="flightDetailHeading">' + name;
        if(name !== null && name.length > 0) result += ':';
        return result + '</td><td class="flightDetailValue">' + value + '</td></tr>';
    };

    function flightRouteLine(description, airport)
    {
        return flightDetailLine(description, airport.code + ' ' + airport.name);
    };

    function flightFirstLastLine(description, first, last, emptyValue)
    {
        return flightDetailLine(description, reportFirstAndLast(first, last, emptyValue));
    };

    function displayFlightPathHandler(sender, args)
    {
        that.highlightFlight(args.index, args.count);
    };

    this.highlightFlight = function(selectIndex, count)
    {
        for(var flightNum = 0;flightNum < count;++flightNum) {
            var element = document.getElementById('flight' + flightNum.toString());
            var className = flightNum != selectIndex ? 'flight' : 'flight flightHighlight';
            element.setAttribute('class', className);
            element.setAttribute('className', className);
        }
    };
}